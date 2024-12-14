using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using AutoMapper;
using LaptopStore.Application.Exceptions;
using LaptopStore.Application.Extensions;
using LaptopStore.Application.Interfaces.Services;
using LaptopStore.Application.Interfaces.Services.Identity;
using LaptopStore.Application.Requests.Identity;
using LaptopStore.Application.Requests.Mail;
using LaptopStore.Application.Responses.Identity;
using LaptopStore.Domain.Entities.Catalog;
using LaptopStore.Infrastructure.Models.Identity;
using LaptopStore.Infrastructure.Specifications;
using LaptopStore.Shared.Constants.Role;
using LaptopStore.Shared.Wrapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Localization;

namespace LaptopStore.Infrastructure.Services.Identity
{
    public class UserService : IUserService
    {
        private readonly UserManager<User> _userManager;
        private readonly RoleManager<Role> _roleManager;
        private readonly IMailService _mailService;
        private readonly IStringLocalizer<UserService> _localizer;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;

        public UserService(
            UserManager<User> userManager,
            IMapper mapper,
            RoleManager<Role> roleManager,
            IMailService mailService,
            IStringLocalizer<UserService> localizer,
            ICurrentUserService currentUserService)
        {
            _userManager = userManager;
            _mapper = mapper;
            _roleManager = roleManager;
            _mailService = mailService;
            _localizer = localizer;
            _currentUserService = currentUserService;
        }

        public async Task<Result<List<UserResponse>>> GetAllAsync()
        {
            var users = await _userManager.Users.ToListAsync();
            var result = _mapper.Map<List<UserResponse>>(users);
            return await Result<List<UserResponse>>.SuccessAsync(result);
        }

        public async Task<IResult> RegisterAsync(RegisterRequest request, string origin)
        {
            var userWithSameUserName = await _userManager.FindByNameAsync(request.UserName);
            if (userWithSameUserName != null)
            {
                return await Result.FailAsync(string.Format(_localizer["Tên người dùng {0} đã được đăng ký."], request.UserName));
            }
            var user = new User
            {
                Email = request.Email,
                FirstName = request.FirstName,
                LastName = request.LastName,
                UserName = request.UserName,
                PhoneNumber = request.PhoneNumber,
                IsActive = request.ActivateUser,
                EmailConfirmed = request.AutoConfirmEmail
            };

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
            {
                var userWithSamePhoneNumber = await _userManager.Users.FirstOrDefaultAsync(x => x.PhoneNumber == request.PhoneNumber);
                if (userWithSamePhoneNumber != null)
                {
                    return await Result.FailAsync(string.Format(_localizer["Số điện thoại {0} đã được đăng ký."], request.PhoneNumber));
                }
            }

            var userWithSameEmail = await _userManager.FindByEmailAsync(request.Email);
            if (userWithSameEmail == null)
            {
                var result = await _userManager.CreateAsync(user, request.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(user, RoleConstants.BasicRole);
                    if (!request.AutoConfirmEmail)
                    {
                        var verificationUri = await SendVerificationEmail(user, origin);
                        var mailRequest = new MailRequest
                        {
                            From = "trhaokiet.1303@gmail.com",
                            To = user.Email,
                            Body = string.Format(_localizer["Xác nhận tài khoản của bạn <a href='{0}'>Nhấp vào đây</a>."], verificationUri),
                            Subject = _localizer["Xác nhận đăng ký tài khoản"]
                        };
                        await Task.Run(() => _mailService.SendAsync(mailRequest));
                        return await Result<string>.SuccessAsync(user.Id, string.Format(_localizer["Người dùng {0} đã được đăng ký. Kiểm tra email để xác nhận đăng ký!"], user.UserName));
                    }
                    return await Result<string>.SuccessAsync(user.Id, string.Format(_localizer["Người dùng {0} đã được đăng ký."], user.UserName));
                }
                else
                {
                    return await Result.FailAsync(result.Errors.Select(a => _localizer[a.Description].ToString()).ToList());
                }
            }
            else
            {
                return await Result.FailAsync(string.Format(_localizer["Email {0} đã được đăng ký."], request.Email));
            }

        }

        private async Task<string> SendVerificationEmail(User user, string origin)
        {
            var code = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var route = "api/identity/user/confirm-email/";
            var endpointUri = new Uri(string.Concat($"{origin}/", route));
            var verificationUri = QueryHelpers.AddQueryString(endpointUri.ToString(), "userId", user.Id);
            verificationUri = QueryHelpers.AddQueryString(verificationUri, "code", code);
            return verificationUri;
        }

        public async Task<IResult<UserResponse>> GetAsync(string userId)
        {
            var user = await _userManager.Users.Where(u => u.Id == userId).FirstOrDefaultAsync();
            var result = _mapper.Map<UserResponse>(user);
            return await Result<UserResponse>.SuccessAsync(result);
        }

        public async Task<IResult> ToggleUserStatusAsync(ToggleUserStatusRequest request)
        {
            var user = await _userManager.Users.Where(u => u.Id == request.UserId).FirstOrDefaultAsync();
            var isAdmin = await _userManager.IsInRoleAsync(user, RoleConstants.AdministratorRole);
            if (isAdmin)
            {
                return await Result.FailAsync(_localizer["Không thể thay đổi trạng thái của tài khoản quản trị viên"]);
            }
            if (user != null)
            {
                user.IsActive = request.ActivateUser;
                user.EmailConfirmed = request.EmailConfirm;
                var identityResult = await _userManager.UpdateAsync(user);
            }
            return await Result.SuccessAsync();
        }

        public async Task<IResult<UserRolesResponse>> GetRolesAsync(string userId)
        {
            var viewModel = new List<UserRoleModel>();
            var user = await _userManager.FindByIdAsync(userId);
            var roles = await _roleManager.Roles.ToListAsync();

            foreach (var role in roles)
            {
                var userRolesViewModel = new UserRoleModel
                {
                    RoleName = role.Name,
                    RoleDescription = role.Description
                };
                if (await _userManager.IsInRoleAsync(user, role.Name))
                {
                    userRolesViewModel.Selected = true;
                }
                else
                {
                    userRolesViewModel.Selected = false;
                }
                viewModel.Add(userRolesViewModel);
            }
            var result = new UserRolesResponse { UserRoles = viewModel };
            return await Result<UserRolesResponse>.SuccessAsync(result);
        }

        public async Task<IResult> UpdateRolesAsync(UpdateUserRolesRequest request)
        {
            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
            {
                return await Result.FailAsync(_localizer["Người dùng không tồn tại."]);
            }
            var roles = await _userManager.GetRolesAsync(user);
            var currentUser = await _userManager.FindByIdAsync(_currentUserService.UserId);
            if (!await _userManager.IsInRoleAsync(currentUser, RoleConstants.AdministratorRole) &&
                !await _userManager.IsInRoleAsync(currentUser, RoleConstants.ManagerUserRole))
            {
                return await Result.FailAsync(_localizer["Bạn không có quyền thực hiện thay đổi vai trò."]);
            }
            if (roles.Contains(RoleConstants.AdministratorRole))
            {
                return await Result.FailAsync(_localizer["Không được phép chỉnh sửa vai trò của Quản trị viên."]);
            }

            var selectedRoles = request.UserRoles.Where(x => x.Selected).Select(y => y.RoleName).ToList();

            if (!selectedRoles.Any())
            {
                return await Result.FailAsync(_localizer["Bạn phải chọn ít nhất một vai trò."]);
            }

            if (selectedRoles.Contains(RoleConstants.AdministratorRole) &&
                !await _userManager.IsInRoleAsync(currentUser, RoleConstants.AdministratorRole))
            {
                return await Result.FailAsync(_localizer["Chỉ Quản trị viên mới được phép gán vai trò Administrator."]);
            }

            if (selectedRoles.Contains(RoleConstants.AdministratorRole) && selectedRoles.Count > 1)
            {
                return await Result.FailAsync(_localizer["Chọn vai trò Administrator, không được chọn thêm vai trò khác."]);
            }
            if (selectedRoles.Contains(RoleConstants.BasicRole) && selectedRoles.Count > 1)
            {
                return await Result.FailAsync(_localizer["Chọn vai trò Basic, không được chọn thêm vai trò khác."]);
            }
            var removeResult = await _userManager.RemoveFromRolesAsync(user, roles);
            if (!removeResult.Succeeded)
            {
                return await Result.FailAsync(_localizer["Không thể xóa vai trò hiện tại."]);
            }
            var addResult = await _userManager.AddToRolesAsync(user, selectedRoles);
            if (!addResult.Succeeded)
            {
                return await Result.FailAsync(_localizer["Không thể thêm vai trò mới."]);
            }
            return await Result.SuccessAsync(_localizer["Cập nhật vai trò thành công."]);
        }


        public async Task<IResult<string>> ConfirmEmailAsync(string userId, string code)
        {
            var user = await _userManager.FindByIdAsync(userId);
            code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code));
            var result = await _userManager.ConfirmEmailAsync(user, code);
            if (result.Succeeded)
            {
                return await Result<string>.SuccessAsync(user.Id, string.Format(_localizer["Tài khoản {0} đã được xác nhận. Bạn có thể sử dụng endpoint /api/identity/token để tạo JWT."], user.Email));
            }
            else
            {
                throw new ApiException(string.Format(_localizer["Đã xảy ra lỗi khi xác nhận {0}"], user.Email));
            }
        }

        public async Task<IResult> ForgotPasswordAsync(ForgotPasswordRequest request, string origin)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                return await Result.FailAsync(_localizer["Đã xảy ra lỗi!"]);
            }

            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));
            var route = "account/reset-password";
            var endpointUri = new Uri(string.Concat($"{origin}/", route));
            var passwordResetURL = QueryHelpers.AddQueryString(endpointUri.ToString(), "Token", code);
            var mailRequest = new MailRequest
            {
                Body = string.Format(_localizer["Vui lòng đặt lại mật khẩu của bạn bằng cách <a href='{0}'>Nhấp vào đây</a>."], HtmlEncoder.Default.Encode(passwordResetURL)),
                Subject = _localizer["Đặt lại mật khẩu"],
                To = request.Email
            };
            await Task.Run(() => _mailService.SendAsync(mailRequest));
            return await Result.SuccessAsync(_localizer["Đã gửi email đặt lại mật khẩu đến email của bạn."]);
        }

        public async Task<IResult> ResetPasswordAsync(ResetPasswordRequest request)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
            {
                return await Result.FailAsync(_localizer["Đã xảy ra lỗi!"]);
            }

            var result = await _userManager.ResetPasswordAsync(user, request.Token, request.Password);
            if (result.Succeeded)
            {
                return await Result.SuccessAsync(_localizer["Đặt lại mật khẩu thành công!"]);
            }
            else
            {
                return await Result.FailAsync(_localizer["Đã xảy ra lỗi!"]);
            }
        }

        public async Task<int> GetCountAsync()
        {
            var count = await _userManager.Users.CountAsync();
            return count;
        }

        public async Task<IResult> DeleteUserAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return await Result.FailAsync(_localizer["Người dùng không tồn tại."]);
            }

            if (!user.EmailConfirmed && !user.IsActive)
            {
                return await Result.FailAsync(_localizer["Không thể xóa tài khoản này vì cả Email xác nhận và trạng thái hoạt động đều không hợp lệ."]);
            }

            if (!user.EmailConfirmed)
            {
                return await Result.FailAsync(_localizer["Không thể xóa tài khoản này vì Email chưa được xác nhận."]);
            }

            if (!user.IsActive)
            {
                return await Result.FailAsync(_localizer["Không thể xóa tài khoản này vì tài khoản không hoạt động."]);
            }

            var isAdmin = await _userManager.IsInRoleAsync(user, RoleConstants.AdministratorRole);
            if (isAdmin)
            {
                return await Result.FailAsync(_localizer["Quản trị viên không thể bị xóa."]);
            }

            user.EmailConfirmed = false;
            user.IsActive = false;
            var updateResult = await _userManager.UpdateAsync(user);

            if (updateResult.Succeeded)
            {
                return await Result.SuccessAsync(_localizer["Người dùng tạm thời bị vô hiệu hóa."]);
            }
            else
            {
                return await Result.FailAsync(updateResult.Errors.Select(a => a.Description).ToList());
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Trava.Shared.Enums
{
    public enum CustomCode
    {
        Success = 2000,
        Created = 2001,
        Updated = 2002,
        Deleted = 2003,
        NoContent = 2004,

        // Error
        ModelInvalid = 4000,
        Unauthorized = 4001,
        Forbidden = 4003,
        SystemError = 5000,

        InvalidToken = 4004,
        EmailAlreadyExists = 4005,
        ProvidedInformationIsInValid = 4006,
        UserNotExists = 4007,
        UserNotConfirmed = 4008,
        InvalidCredentials = 4009,
        UserAccountLocked = 4010,
        ConfirmEmailTokenInvalidOrExpired = 4011,
        UserAlreadyConfirmed = 4012,
        NewPasswordSameAsOld = 4013,
        OtpInvalidOrExpired = 4014,
        TwoFactorIsAlreadyEnabled = 4015,
        TwoFactorIsAlreadyDisabled = 4016,
        UserIdNotFound = 4017,
        AccessTokenInvalidOrExpired = 4018,
        UnauthorizedAction = 4019,

        //Space Error
        SpaceNotFound = 4100,
        SpaceNameAlreadyExists = 4101,

        //TaskItem Error
        TaskItemNotFound = 4200,
        ParentTaskItemNotFound = 4201,
        TaskItemTitleAlreadyExists = 4202,
        ParentTaskItemNotExistInSpace = 4203,
        AssignedUserNotInSpace = 4204,
        

        //Space Invitation Error
        SpaceInvitationNotFound = 4300,
        InvalidInvitationStatusTransition = 4301,
    }
}
# Summary

|||
|:---|:---|
| Generated on: | 21-11-2025 - 12:10:01 |
| Coverage date: | 21-11-2025 - 11:47:16 - 21-11-2025 - 11:54:12 |
| Parser: | MultiReport (3x Cobertura) |
| Assemblies: | 1 |
| Classes: | 40 |
| Files: | 33 |
| **Line coverage:** | 18% (865 of 4801) |
| Covered lines: | 865 |
| Uncovered lines: | 3936 |
| Coverable lines: | 4801 |
| Total lines: | 6025 |
| **Branch coverage:** | 31.5% (172 of 545) |
| Covered branches: | 172 |
| Total branches: | 545 |
| **Method coverage:** | [Feature is only available for sponsors](https://reportgenerator.io/pro) |

# Risk Hotspots

| **Assembly** | **Class** | **Method** | **Crap Score** | **Cyclomatic complexity** |
|:---|:---|:---|---:|---:|
| v2 | v2.Controller.VehicleController | UpdateVehicle() | 506 | 22 || v2 | v2.Controllers.ReservationController | Update() | 420 | 20 || v2 | v2.infrastructure.Services.ReservationService | UpdateReservationForUserAsync() | 420 | 20 || v2 | v2.Controller.VehicleController | CreateVehicle() | 342 | 18 || v2 | v2.Controller.VehicleController | StartSessionByEntry() | 342 | 18 || v2 | V2.Controllers.PaymentController | RefundPayment() | 272 | 16 || v2 | v2.infrastructure.Services.ParkingLotService | StartSessionAsync() | 210 | 14 || v2 | V2.Controllers.PaymentController | ConfirmPayment() | 182 | 13 || v2 | v2.Controller.VehicleController | DeleteVehicle() | 156 | 12 || v2 | v2.Controller.AuthController | Login() | 110 | 10 || v2 | v2.Controller.AuthController | Register() | 110 | 10 || v2 | v2.Controllers.ParkingLotsController | Create() | 110 | 10 || v2 | V2.Controllers.PaymentController | CreatePayment() | 110 | 10 || v2 | v2.infrastructure.Services.VehicleService | StartSessionByEntryAsync() | 110 | 10 || v2 | Program | <Main>$(...) | 72 | 8 || v2 | v2.Controller.AuthController | Profile() | 72 | 8 || v2 | v2.Controller.AuthController | Profile() | 72 | 8 || v2 | v2.Controller.VehicleController | GetAllVehicles() | 72 | 8 || v2 | v2.Controller.VehicleController | GetAllVehiclesforUser() | 72 | 8 || v2 | v2.Controller.VehicleController | GetReservationsByVehicle() | 72 | 8 || v2 | v2.Controller.VehicleController | GetVehicleHistory() | 72 | 8 || v2 | v2.Controllers.ParkingLotsController | Update() | 72 | 8 || v2 | V2.Controllers.PaymentController | GetPaymentsByUsername() | 72 | 8 || v2 | V2.Controllers.PaymentController | PartialUpdatePayment() | 72 | 8 || v2 | v2.Infrastructure.Services.PaymentService | RefundPaymentAsync() | 61 | 12 || v2 | v2.Controller.AuthController | Logout() | 42 | 6 || v2 | V2.Controllers.PaymentController | GetPayments() | 42 | 6 || v2 | v2.infrastructure.Services.ParkingLotService | StopSessionAsync() | 42 | 6 || v2 | v2.infrastructure.Services.ParkingLotService | UpdateParkingLotAsync() | 29 | 24 || v2 | v2.Infrastructure.Services.PaymentService | UpdatePaymentAsync() | 24 | 24 || v2 | v2.Infrastructure.Services.PaymentService | CreatePaymentAsync() | 18 | 18 || v2 | v2.infrastructure.Services.VehicleService | UpdateVehicleAsync() | 19 | 18 |
# Coverage

| **Name** | **Covered** | **Uncovered** | **Coverable** | **Total** | **Line coverage** | **Covered** | **Total** | **Branch coverage** |
|:---|---:|---:|---:|---:|---:|---:|---:|---:|
| **v2** | **865** | **3936** | **4801** | **6418** | **18%** | **172** | **545** | **31.5%** |
| Program | 0 | 70 | 70 | 148 | 0% | 0 | 8 | 0% |
| TokenBlacklist | 0 | 7 | 7 | 14 | 0% | 0 | 0 |  |
| v2.Controller.AuthController | 0 | 63 | 63 | 109 | 0% | 0 | 42 | 0% |
| v2.Controller.VehicleController | 0 | 128 | 128 | 199 | 0% | 0 | 102 | 0% |
| v2.Controllers.ParkingLotsController | 0 | 69 | 69 | 133 | 0% | 0 | 26 | 0% |
| V2.Controllers.PaymentController | 0 | 92 | 92 | 142 | 0% | 0 | 61 | 0% |
| v2.Controllers.ReservationController | 23 | 63 | 86 | 129 | 26.7% | 2 | 32 | 6.2% |
| v2.Core.DTOs.ConfirmPaymentRequestDTO | 2 | 0 | 2 | 62 | 100% | 0 | 0 |  |
| v2.Core.DTOs.CreatePaymentRequestDTO | 8 | 0 | 8 | 62 | 100% | 0 | 0 |  |
| v2.Core.DTOs.CreateVehicleDto | 5 | 0 | 5 | 25 | 100% | 0 | 0 |  |
| v2.Core.DTOs.LoginDto | 2 | 0 | 2 | 16 | 100% | 0 | 0 |  |
| v2.Core.DTOs.ParkingLotCreateRequest | 8 | 0 | 8 | 13 | 100% | 0 | 0 |  |
| v2.Core.DTOs.PaymentResponseDTO | 12 | 0 | 12 | 62 | 100% | 0 | 0 |  |
| v2.Core.DTOs.ProfileDto | 10 | 0 | 10 | 14 | 100% | 0 | 0 |  |
| v2.Core.DTOs.RefundPaymentRequestDTO | 2 | 0 | 2 | 62 | 100% | 0 | 0 |  |
| v2.Core.DTOs.RegisterDto | 4 | 0 | 4 | 16 | 100% | 0 | 0 |  |
| v2.Core.DTOs.ReservationCreateRequest | 5 | 0 | 5 | 33 | 100% | 0 | 0 |  |
| v2.Core.DTOs.ReservationResponse | 9 | 0 | 9 | 33 | 100% | 0 | 0 |  |
| v2.Core.DTOs.ReservationUpdateRequest | 0 | 3 | 3 | 33 | 0% | 0 | 0 |  |
| v2.Core.DTOs.SessionStartRequest | 0 | 2 | 2 | 13 | 0% | 0 | 0 |  |
| v2.Core.DTOs.SessionStopRequest | 0 | 2 | 2 | 13 | 0% | 0 | 0 |  |
| v2.Core.DTOs.UpdatePaymentRequestDTO | 8 | 0 | 8 | 62 | 100% | 0 | 0 |  |
| v2.Core.DTOs.UpdateVehicleDto | 6 | 0 | 6 | 25 | 100% | 0 | 0 |  |
| v2.Core.DTOs.UpdateVehicleEntryDto | 0 | 1 | 1 | 25 | 0% | 0 | 0 |  |
| v2.Core.Models.ParkingLot | 14 | 0 | 14 | 20 | 100% | 0 | 0 |  |
| v2.Core.Models.Payment | 13 | 1 | 14 | 22 | 92.8% | 0 | 0 |  |
| v2.Core.Models.Reservation | 14 | 0 | 14 | 23 | 100% | 0 | 0 |  |
| v2.Core.Models.Session | 0 | 14 | 14 | 28 | 0% | 0 | 0 |  |
| v2.Core.Models.User | 15 | 0 | 15 | 25 | 100% | 0 | 0 |  |
| v2.Core.Models.Vehicle | 11 | 0 | 11 | 17 | 100% | 0 | 0 |  |
| v2.Infrastructure.Data.ApplicationDbContext | 142 | 0 | 142 | 165 | 100% | 0 | 0 |  |
| v2.infrastructure.Services.AuthService | 133 | 30 | 163 | 237 | 81.5% | 31 | 44 | 70.4% |
| v2.infrastructure.Services.ParkingLotService | 143 | 171 | 314 | 447 | 45.5% | 26 | 56 | 46.4% |
| v2.Infrastructure.Services.PaymentService | 133 | 15 | 148 | 217 | 89.8% | 61 | 78 | 78.2% |
| v2.infrastructure.Services.ReservationService | 31 | 63 | 94 | 146 | 32.9% | 9 | 40 | 22.5% |
| v2.infrastructure.Services.VehicleService | 112 | 66 | 178 | 268 | 62.9% | 43 | 56 | 76.7% |
| v2.Migrations.ApplicationDbContextModelSnapshot | 0 | 655 | 655 | 701 | 0% | 0 | 0 |  |
| v2.Migrations.InitialCreate | 0 | 1067 | 1067 | 1173 | 0% | 0 | 0 |  |
| v2.Migrations.MakePaymentSessionIDNullable | 0 | 679 | 679 | 745 | 0% | 0 | 0 |  |
| v2.Migrations.MakeUserIdNullableInSession | 0 | 675 | 675 | 741 | 0% | 0 | 0 |  |


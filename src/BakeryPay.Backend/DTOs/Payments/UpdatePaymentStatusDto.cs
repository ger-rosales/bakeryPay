using BakeryPay.Backend.Enums;

namespace BakeryPay.Backend.DTOs.Payments;

public class UpdatePaymentStatusDto
{
    public PaymentStatus Status { get; set; }
}

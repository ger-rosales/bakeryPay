using BakeryPay.Domain.Enums;

namespace BakeryPay.Application.DTOs.Payments;

public class UpdatePaymentStatusDto
{
    public PaymentStatus Status { get; set; }
}

namespace BakeryPay.Backend.Common;

public class EmailSendResult
{
    public bool Success { get; init; }
    public bool IsConfigured { get; init; }
    public string Message { get; init; } = string.Empty;

    public static EmailSendResult Sent(string message = "Correo enviado correctamente.") =>
        new()
        {
            Success = true,
            IsConfigured = true,
            Message = message
        };

    public static EmailSendResult NotConfigured(string message) =>
        new()
        {
            Success = false,
            IsConfigured = false,
            Message = message
        };

    public static EmailSendResult Failed(string message) =>
        new()
        {
            Success = false,
            IsConfigured = true,
            Message = message
        };
}

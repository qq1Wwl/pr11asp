namespace pr9ASP.Services
{
    // Сервис не знает про HTTP: он сообщает о проблеме типом исключения,
    // а статус-код (400 / 409) выбирает контроллер.

    /// <summary>Связанная сущность (по Id из DTO) не найдена → 400.</summary>
    public class ReferenceNotFoundException : InvalidOperationException
    {
        public ReferenceNotFoundException(string message) : base(message) { }
    }

    /// <summary>Бизнес-правило не позволяет выполнить действие в текущем состоянии → 409.</summary>
    public class ConflictException : InvalidOperationException
    {
        public ConflictException(string message) : base(message) { }
    }
}

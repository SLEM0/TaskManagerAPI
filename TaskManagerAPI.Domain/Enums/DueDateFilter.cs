namespace TaskManagerAPI.Domain.Enums;

public enum DueDateFilter
{
    NoDate,          // Без срока (не изменился)
    Expired,         // Просроченные (учитывает время)
    DueWithinDay,    // Истекает в течение суток (24 часа)
    DueWithinWeek,   // Истекает в течение недели (7 дней)
    DueWithinMonth   // Истекает в течение месяца (30 дней)
}
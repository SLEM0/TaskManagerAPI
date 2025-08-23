namespace TaskManagerAPI.Domain.Enums;

public enum DueDateFilter
{
    Expired,        // Просроченные
    DueToday,       // Сегодня
    DueTomorrow,    // Завтра
    ThisWeek,       // На этой неделе
    NextWeek,       // На следующей неделе
    ThisMonth,      // В этом месяце
    NoDate          // Без срока
}
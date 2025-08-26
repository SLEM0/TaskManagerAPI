namespace TaskManagerAPI.Application.Utils;

public static class SystemMessages
{
    public static string ChangedTitle(string newTitle) => $"changed title to '{newTitle}'";
    public static string ChangedDescription() => "changed description";
    public static string ChangedDueDate(DateTime? newDueDate) =>
        newDueDate.HasValue ? $"changed due date to {newDueDate:yyyy-MM-dd}" : "removed due date";
    public static string MarkedAsCompleted() => "marked as completed";
    public static string MarkedAsIncomplete() => "marked as incomplete";
    public static string MovedToList(string listName) => $"moved to list '{listName}'";
    public static string AssignedUser(string userName) => $"assigned {userName}";
    public static string UnassignedUser(string userName) => $"unassigned {userName}";
    public static string AddedLabel(string labelName) => $"added label '{labelName}'";
    public static string RemovedLabel(string labelName) => $"removed label '{labelName}'";
}
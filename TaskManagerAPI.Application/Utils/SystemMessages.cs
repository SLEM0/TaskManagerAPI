namespace TaskManagerAPI.Application.Utils;

public static class SystemMessages
{
    public static string ChangedTitle(string userName, string newTitle)
        => $"{userName} changed title to '{newTitle}'";

    public static string ChangedDescription(string userName)
        => $"{userName} changed description";

    public static string ChangedDueDate(string userName, DateTime? newDueDate)
        => newDueDate.HasValue
            ? $"{userName} set due date to {newDueDate:yyyy-MM-dd}"
            : $"{userName} removed due date";

    public static string MarkedAsCompleted(string userName)
        => $"{userName} marked task as completed";

    public static string MarkedAsIncomplete(string userName)
        => $"{userName} marked task as incomplete";

    public static string MovedToList(string userName, string listName)
        => $"{userName} moved task to '{listName}'";

    public static string AssignedUser(string userName, string assignedUserName)
        => $"{userName} assigned {assignedUserName} to task";

    public static string UnassignedUser(string userName, string unassignedUserName)
        => $"{userName} unassigned {unassignedUserName} from task";

    public static string AddedLabel(string userName, string labelName)
        => $"{userName} added label '{labelName}'";

    public static string RemovedLabel(string userName, string labelName)
        => $"{userName} removed label '{labelName}'";

    public static string AddedAttachment(string userName, string fileName)
        => $"{userName} added attachment '{fileName}'";

    public static string RemovedAttachment(string userName, string fileName)
        => $"{userName} removed attachment '{fileName}'";

    public static string CreatedTask(string userName)
        => $"{userName} created task";
}
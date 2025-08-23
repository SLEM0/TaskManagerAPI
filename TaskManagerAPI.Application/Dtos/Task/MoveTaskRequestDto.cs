namespace TaskManagerAPI.Application.Dtos.Task;

public class MoveTaskRequestDto
{
    public int NewListId { get; set; }
    public int NewOrder { get; set; } // Новая позиция (целое число)
}
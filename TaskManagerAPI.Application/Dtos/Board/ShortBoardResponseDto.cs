using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TaskManagerAPI.Application.Dtos.Label;
using TaskManagerAPI.Application.Dtos.Member;
using TaskManagerAPI.Application.Dtos.TaskList;

namespace TaskManagerAPI.Application.Dtos.Board;

public class ShortBoardResponseDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateTime CreatedAt { get; set; }
    public bool IsOwner { get; set; }
    public int OwnerId { get; set; }
    public string OwnerName { get; set; }
}
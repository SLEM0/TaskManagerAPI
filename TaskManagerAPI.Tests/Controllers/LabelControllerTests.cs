//using Microsoft.AspNetCore.Mvc;
//using Moq;
//using TaskManagerAPI.Application.Interfaces;
//using TaskManagerAPI.Controllers;
//using TaskManagerAPI.Dtos.Label;
//using TaskManagerAPI.Dtos.Task;

//namespace TaskManagerAPI.Tests.Controllers;

//public class LabelControllerTests
//{
//    private readonly Mock<ILabelService> _mockLabelService;
//    private readonly Mock<IUserContext> _mockUserContext;
//    private readonly LabelController _controller;

//    public LabelControllerTests()
//    {
//        _mockLabelService = new Mock<ILabelService>();
//        _mockUserContext = new Mock<IUserContext>();
//        _controller = new LabelController(_mockLabelService.Object, _mockUserContext.Object);
//    }

//    [Fact]
//    public async Task GetBoardLabels_WhenAuthorized_ReturnsOkResultWithLabels()
//    {
//        // Arrange
//        var boardId = 1;
//        var userId = 1;
//        var expectedLabels = new List<LabelDto>
//    {
//        new LabelDto { Id = 1, Name = "Label 1", Color = "#FF0000", TaskCount = 0 },
//        new LabelDto { Id = 2, Name = "Label 2", Color = "#00FF00", TaskCount = 3 }
//    };

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockLabelService.Setup(x => x.GetBoardLabelsAsync(boardId, userId))
//            .ReturnsAsync(expectedLabels);

//        // Act
//        var result = await _controller.GetBoardLabels(boardId);

//        // Assert
//        var actionResult = Assert.IsType<ActionResult<IEnumerable<LabelDto>>>(result);
//        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
//        var labels = Assert.IsAssignableFrom<IEnumerable<LabelDto>>(okResult.Value);
//        Assert.Equal(2, labels.Count());
//    }

//    [Fact]
//    public async Task GetBoardLabels_WhenNotAuthorized_ReturnsForbid()
//    {
//        // Arrange
//        var boardId = 1;
//        var userId = 1;

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockLabelService.Setup(x => x.GetBoardLabelsAsync(boardId, userId))
//            .ThrowsAsync(new UnauthorizedAccessException());

//        // Act
//        var result = await _controller.GetBoardLabels(boardId);

//        // Assert
//        var actionResult = Assert.IsType<ActionResult<IEnumerable<LabelDto>>>(result);
//        Assert.IsType<ForbidResult>(actionResult.Result);
//    }

//    [Fact]
//    public async Task GetLabel_WhenExistsAndAuthorized_ReturnsOkResult()
//    {
//        // Arrange
//        var boardId = 1;
//        var labelId = 1;
//        var userId = 1;
//        var expectedLabel = new LabelDetailsDto
//        {
//            Id = labelId,
//            Name = "Test Label",
//            Color = "#FF0000",
//            BoardId = boardId
//        };

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockLabelService.Setup(x => x.GetLabelDetailsAsync(labelId, userId))
//            .ReturnsAsync(expectedLabel);

//        // Act
//        var result = await _controller.GetLabel(boardId, labelId);

//        // Assert
//        var actionResult = Assert.IsType<ActionResult<LabelDetailsDto>>(result);
//        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
//        var label = Assert.IsType<LabelDetailsDto>(okResult.Value);
//        Assert.Equal(labelId, label.Id);
//    }

//    [Fact]
//    public async Task GetLabel_WhenNotFound_ReturnsNotFound()
//    {
//        // Arrange
//        var boardId = 1;
//        var labelId = 1;
//        var userId = 1;

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockLabelService.Setup(x => x.GetLabelDetailsAsync(labelId, userId))
//            .ThrowsAsync(new KeyNotFoundException());

//        // Act
//        var result = await _controller.GetLabel(boardId, labelId);

//        // Assert
//        var actionResult = Assert.IsType<ActionResult<LabelDetailsDto>>(result);
//        Assert.IsType<NotFoundResult>(actionResult.Result);
//    }

//    [Fact]
//    public async Task CreateLabel_WhenAuthorized_ReturnsCreatedAtActionResult()
//    {
//        // Arrange
//        var boardId = 1;
//        var userId = 1;
//        var createDto = new CreateLabelDto { Name = "New Label", Color = "#0000FF" };
//        var expectedLabel = new LabelDto
//        {
//            Id = 1,
//            Name = "New Label",
//            Color = "#0000FF",
//            TaskCount = 0
//        };

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockLabelService.Setup(x => x.CreateLabelAsync(createDto, boardId, userId))
//            .ReturnsAsync(expectedLabel);

//        // Act
//        var result = await _controller.CreateLabel(boardId, createDto);

//        // Assert
//        var actionResult = Assert.IsType<ActionResult<LabelDto>>(result);
//        var createdAtResult = Assert.IsType<CreatedAtActionResult>(actionResult.Result);
//        Assert.Equal(nameof(LabelController.GetLabel), createdAtResult.ActionName);
//        Assert.Equal(boardId, createdAtResult.RouteValues["boardId"]);
//        Assert.Equal(expectedLabel.Id, createdAtResult.RouteValues["labelId"]);

//        var label = Assert.IsType<LabelDto>(createdAtResult.Value);
//        Assert.Equal("New Label", label.Name);
//    }

//    [Fact]
//    public async Task CreateLabel_WhenNotAuthorized_ReturnsForbid()
//    {
//        // Arrange
//        var boardId = 1;
//        var userId = 1;
//        var createDto = new CreateLabelDto { Name = "New Label", Color = "#0000FF" };

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockLabelService.Setup(x => x.CreateLabelAsync(createDto, boardId, userId))
//            .ThrowsAsync(new UnauthorizedAccessException());

//        // Act
//        var result = await _controller.CreateLabel(boardId, createDto);

//        // Assert
//        var actionResult = Assert.IsType<ActionResult<LabelDto>>(result);
//        Assert.IsType<ForbidResult>(actionResult.Result);
//    }

//    [Fact]
//    public async Task UpdateLabel_WhenAuthorized_ReturnsNoContent()
//    {
//        // Arrange
//        var boardId = 1;
//        var labelId = 1;
//        var userId = 1;
//        var updateDto = new UpdateLabelDto { Name = "Updated Label", Color = "#00FF00" };

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockLabelService.Setup(x => x.UpdateLabelAsync(labelId, updateDto, userId))
//            .Returns(Task.CompletedTask);

//        // Act
//        var result = await _controller.UpdateLabel(boardId, labelId, updateDto);

//        // Assert
//        Assert.IsType<NoContentResult>(result);
//    }

//    [Fact]
//    public async Task UpdateLabel_WhenNotFound_ReturnsNotFound()
//    {
//        // Arrange
//        var boardId = 1;
//        var labelId = 1;
//        var userId = 1;
//        var updateDto = new UpdateLabelDto { Name = "Updated Label", Color = "#00FF00" };

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockLabelService.Setup(x => x.UpdateLabelAsync(labelId, updateDto, userId))
//            .ThrowsAsync(new KeyNotFoundException());

//        // Act
//        var result = await _controller.UpdateLabel(boardId, labelId, updateDto);

//        // Assert
//        Assert.IsType<NotFoundResult>(result);
//    }

//    [Fact]
//    public async Task DeleteLabel_WhenAuthorized_ReturnsNoContent()
//    {
//        // Arrange
//        var boardId = 1;
//        var labelId = 1;
//        var userId = 1;

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockLabelService.Setup(x => x.DeleteLabelAsync(labelId, userId))
//            .Returns(Task.CompletedTask);

//        // Act
//        var result = await _controller.DeleteLabel(boardId, labelId);

//        // Assert
//        Assert.IsType<NoContentResult>(result);
//    }

//    [Fact]
//    public async Task GetLabelTasks_WhenAuthorized_ReturnsOkResultWithTasks()
//    {
//        // Arrange
//        var boardId = 1;
//        var labelId = 1;
//        var userId = 1;
//        var expectedTasks = new List<TaskWithDetailsDto>
//    {
//        new TaskWithDetailsDto { Id = 1, Title = "Task 1", Description = "Desc 1" },
//        new TaskWithDetailsDto { Id = 2, Title = "Task 2", Description = "Desc 2" }
//    };

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockLabelService.Setup(x => x.GetLabelTasksAsync(labelId, userId))
//            .ReturnsAsync(expectedTasks);

//        // Act
//        var result = await _controller.GetLabelTasks(boardId, labelId);

//        // Assert
//        var actionResult = Assert.IsType<ActionResult<IEnumerable<TaskWithDetailsDto>>>(result);
//        var okResult = Assert.IsType<OkObjectResult>(actionResult.Result);
//        var tasks = Assert.IsAssignableFrom<IEnumerable<TaskWithDetailsDto>>(okResult.Value);
//        Assert.Equal(2, tasks.Count());
//    }

//    [Fact]
//    public async Task GetLabelTasks_WhenLabelNotFound_ReturnsNotFoundWithMessage()
//    {
//        // Arrange
//        var boardId = 1;
//        var labelId = 1;
//        var userId = 1;

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockLabelService.Setup(x => x.GetLabelTasksAsync(labelId, userId))
//            .ThrowsAsync(new KeyNotFoundException("Label not found"));

//        // Act
//        var result = await _controller.GetLabelTasks(boardId, labelId);

//        // Assert
//        var actionResult = Assert.IsType<ActionResult<IEnumerable<TaskWithDetailsDto>>>(result);
//        var notFoundResult = Assert.IsType<NotFoundObjectResult>(actionResult.Result);
//        Assert.Equal("Label not found", notFoundResult.Value);
//    }

//    [Fact]
//    public async Task GetLabelTasks_WhenNotAuthorized_ReturnsForbid()
//    {
//        // Arrange
//        var boardId = 1;
//        var labelId = 1;
//        var userId = 1;

//        _mockUserContext.Setup(x => x.GetCurrentUserId()).Returns(userId);
//        _mockLabelService.Setup(x => x.GetLabelTasksAsync(labelId, userId))
//            .ThrowsAsync(new UnauthorizedAccessException());

//        // Act
//        var result = await _controller.GetLabelTasks(boardId, labelId);

//        // Assert
//        var actionResult = Assert.IsType<ActionResult<IEnumerable<TaskWithDetailsDto>>>(result);
//        Assert.IsType<ForbidResult>(actionResult.Result);
//    }
//}
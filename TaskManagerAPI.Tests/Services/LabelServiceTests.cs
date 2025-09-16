using AutoMapper;
using Moq;
using TaskManagerAPI.Application.Dtos.Label;
using TaskManagerAPI.Application.Exceptions;
using TaskManagerAPI.Application.Interfaces.Repositories;
using TaskManagerAPI.Application.Interfaces.Services;
using TaskManagerAPI.Domain.Entities;
using TaskManagerAPI.Domain.Enums;
using TaskManagerAPI.Infrastructure.Services;

namespace TaskManagerAPI.Tests.Services;

public class LabelServiceTests
{
    private readonly Mock<ILabelRepository> _labelRepositoryMock;
    private readonly Mock<ICheckAccessService> _authServiceMock;
    private readonly IMapper _mapper;
    private readonly LabelService _labelService;

    public LabelServiceTests()
    {
        _labelRepositoryMock = new Mock<ILabelRepository>();
        _authServiceMock = new Mock<ICheckAccessService>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Label, LabelResponseDto>();
        });
        _mapper = config.CreateMapper();

        _labelService = new LabelService(
            _labelRepositoryMock.Object,
            _authServiceMock.Object,
            _mapper);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetLabelDetailsAsync_ValidRequest_ReturnsLabelDetails()
    {
        // Arrange
        var labelId = 1;
        var boardId = 1;
        var label = new Label
        {
            Id = labelId,
            Name = "Test Label",
            Color = "#FF0000",
            BoardId = boardId
        };

        _labelRepositoryMock.Setup(x => x.GetByIdWithBoardAsync(labelId))
            .ReturnsAsync(label);
        _authServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Viewer))
            .ReturnsAsync((true, true));

        // Act
        var result = await _labelService.GetLabelDetailsAsync(labelId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(label.Id, result.Id);
        Assert.Equal(label.Name, result.Name);
        Assert.Equal(label.Color, result.Color);
        _labelRepositoryMock.Verify(x => x.GetByIdWithBoardAsync(labelId), Times.Once);
        _authServiceMock.Verify(x => x.CheckBoardAccessAsync(boardId, BoardRole.Viewer), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task GetLabelDetailsAsync_LabelNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var labelId = 1;

        _labelRepositoryMock.Setup(x => x.GetByIdWithBoardAsync(labelId))
            .ReturnsAsync((Label)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _labelService.GetLabelDetailsAsync(labelId));
    }

    [Fact]
    public async System.Threading.Tasks.Task GetLabelDetailsAsync_NoAccess_ThrowsForbiddenAccessException()
    {
        // Arrange
        var labelId = 1;
        var boardId = 1;
        var label = new Label
        {
            Id = labelId,
            BoardId = boardId
        };

        _labelRepositoryMock.Setup(x => x.GetByIdWithBoardAsync(labelId))
            .ReturnsAsync(label);
        _authServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Viewer))
            .ReturnsAsync((false, false));

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            _labelService.GetLabelDetailsAsync(labelId));
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateLabelAsync_ValidRequest_ReturnsCreatedLabel()
    {
        // Arrange
        var boardId = 1;
        var labelDto = new LabelRequestDto
        {
            Name = "New Label",
            Color = "#00FF00"
        };
        var label = new Label
        {
            Id = 1,
            Name = labelDto.Name,
            Color = labelDto.Color,
            BoardId = boardId
        };

        _authServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
            .ReturnsAsync((true, true));
        _labelRepositoryMock.Setup(x => x.AddAsync(It.IsAny<Label>()))
            .Callback<Label>(l => l.Id = 1);
        _labelRepositoryMock.Setup(x => x.GetByIdWithBoardAsync(1))
            .ReturnsAsync(label);

        // Act
        var result = await _labelService.CreateLabelAsync(labelDto, boardId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(label.Id, result.Id);
        Assert.Equal(label.Name, result.Name);
        Assert.Equal(label.Color, result.Color);
        _authServiceMock.Verify(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor), Times.Once);
        _labelRepositoryMock.Verify(x => x.AddAsync(It.IsAny<Label>()), Times.Once);
        _labelRepositoryMock.Verify(x => x.GetByIdWithBoardAsync(1), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task CreateLabelAsync_NoAccess_ThrowsForbiddenAccessException()
    {
        // Arrange
        var boardId = 1;
        var labelDto = new LabelRequestDto
        {
            Name = "New Label",
            Color = "#00FF00"
        };

        _authServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
            .ReturnsAsync((false, false));

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            _labelService.CreateLabelAsync(labelDto, boardId));
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateLabelAsync_ValidRequest_ReturnsUpdatedLabel()
    {
        // Arrange
        var labelId = 1;
        var boardId = 1;
        var labelDto = new LabelRequestDto
        {
            Name = "Updated Label",
            Color = "#0000FF"
        };
        var existingLabel = new Label
        {
            Id = labelId,
            Name = "Old Label",
            Color = "#FF0000",
            BoardId = boardId
        };
        var updatedLabel = new Label
        {
            Id = labelId,
            Name = labelDto.Name,
            Color = labelDto.Color,
            BoardId = boardId
        };

        _labelRepositoryMock.Setup(x => x.GetByIdWithBoardAsync(labelId))
            .ReturnsAsync(existingLabel);
        _authServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
            .ReturnsAsync((true, true));
        _labelRepositoryMock.Setup(x => x.UpdateAsync(It.IsAny<Label>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);
        _labelRepositoryMock.Setup(x => x.GetByIdWithBoardAsync(labelId))
            .ReturnsAsync(updatedLabel);

        // Act
        var result = await _labelService.UpdateLabelAsync(labelId, labelDto);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(labelDto.Name, result.Name);
        Assert.Equal(labelDto.Color, result.Color);
        _labelRepositoryMock.Verify(x => x.GetByIdWithBoardAsync(labelId), Times.AtLeastOnce);
        _authServiceMock.Verify(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor), Times.Once);
        _labelRepositoryMock.Verify(x => x.UpdateAsync(It.IsAny<Label>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateLabelAsync_LabelNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var labelId = 1;
        var labelDto = new LabelRequestDto
        {
            Name = "Updated Label",
            Color = "#0000FF"
        };

        _labelRepositoryMock.Setup(x => x.GetByIdWithBoardAsync(labelId))
            .ReturnsAsync((Label)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _labelService.UpdateLabelAsync(labelId, labelDto));
    }

    [Fact]
    public async System.Threading.Tasks.Task UpdateLabelAsync_NoAccess_ThrowsForbiddenAccessException()
    {
        // Arrange
        var labelId = 1;
        var boardId = 1;
        var labelDto = new LabelRequestDto
        {
            Name = "Updated Label",
            Color = "#0000FF"
        };
        var label = new Label
        {
            Id = labelId,
            BoardId = boardId
        };

        _labelRepositoryMock.Setup(x => x.GetByIdWithBoardAsync(labelId))
            .ReturnsAsync(label);
        _authServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
            .ReturnsAsync((false, false));

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            _labelService.UpdateLabelAsync(labelId, labelDto));
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteLabelAsync_ValidRequest_DeletesLabel()
    {
        // Arrange
        var labelId = 1;
        var boardId = 1;
        var label = new Label
        {
            Id = labelId,
            BoardId = boardId
        };

        _labelRepositoryMock.Setup(x => x.GetByIdWithBoardAsync(labelId))
            .ReturnsAsync(label);
        _authServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
            .ReturnsAsync((true, true));
        _labelRepositoryMock.Setup(x => x.DeleteAsync(It.IsAny<Label>()))
            .Returns(System.Threading.Tasks.Task.CompletedTask);

        // Act
        await _labelService.DeleteLabelAsync(labelId);

        // Assert
        _labelRepositoryMock.Verify(x => x.GetByIdWithBoardAsync(labelId), Times.Once);
        _authServiceMock.Verify(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor), Times.Once);
        _labelRepositoryMock.Verify(x => x.DeleteAsync(It.IsAny<Label>()), Times.Once);
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteLabelAsync_LabelNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var labelId = 1;

        _labelRepositoryMock.Setup(x => x.GetByIdWithBoardAsync(labelId))
            .ReturnsAsync((Label)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _labelService.DeleteLabelAsync(labelId));
    }

    [Fact]
    public async System.Threading.Tasks.Task DeleteLabelAsync_NoAccess_ThrowsForbiddenAccessException()
    {
        // Arrange
        var labelId = 1;
        var boardId = 1;
        var label = new Label
        {
            Id = labelId,
            BoardId = boardId
        };

        _labelRepositoryMock.Setup(x => x.GetByIdWithBoardAsync(labelId))
            .ReturnsAsync(label);
        _authServiceMock.Setup(x => x.CheckBoardAccessAsync(boardId, BoardRole.Editor))
            .ReturnsAsync((false, false));

        // Act & Assert
        await Assert.ThrowsAsync<ForbiddenAccessException>(() =>
            _labelService.DeleteLabelAsync(labelId));
    }
}
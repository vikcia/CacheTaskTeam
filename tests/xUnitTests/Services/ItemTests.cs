using AutoFixture.Xunit2;
using AutoFixture;
using Domain.Entities;
using FluentAssertions;
using Moq;
using Xunit;
using Domain.Interfaces;
using Application.Services;
using Domain.Exceptions;
using Application.Dto;

namespace xUnitTests.Services
{
    public class ItemTests
    {
        private readonly Mock<IItemRepository> _itemRepositoryMock;
        private readonly ItemService _itemServiceMock;
        private readonly Fixture _fixture;

        public ItemTests()
        {
            _itemRepositoryMock = new Mock<IItemRepository>();
            _itemServiceMock = new ItemService(_itemRepositoryMock.Object);
            _fixture = new Fixture();
        }

        [Fact]
        public async Task Get_ReturnsItems_WhenRepositoryReturnsData()
        {
            // Arrange
            var itemEntities = new List<ItemEntity>
        {
            new ItemEntity { Key = "key1", Value = "[\"value1\"]", ExpirationPeriod = 60, ExpirationDate = DateTime.Now.AddSeconds(60) },
            new ItemEntity { Key = "key2", Value = "[\"value2\"]", ExpirationPeriod = 120, ExpirationDate = DateTime.Now.AddSeconds(120) }
        };

            //var itemRepositoryMock = new Mock<IItemRepository>();
            _itemRepositoryMock.Setup(repo => repo.Get()).ReturnsAsync(itemEntities);

            // Act
            var result = await _itemServiceMock.Get();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(2);
        }

        [Fact]
        public async Task Get_ThrowsNotFoundException_WhenRepositoryReturnsNoData()
        {
            // Arrange
            _itemRepositoryMock.Setup(repo => repo.Get()).ReturnsAsync(new List<ItemEntity>());

            // Act & Assert
            await Assert.ThrowsAsync<NotFoundException>(() => _itemServiceMock.Get());
        }

        [Fact]
        public async Task GetByKey_ReturnsItem_WhenRepositoryReturnsData()
        {
            // Arrange
            var itemEntity = new ItemEntity { Key = "key1", Value = "[\"value1\"]", ExpirationPeriod = 60, ExpirationDate = DateTime.Now.AddSeconds(60) };

            var itemRepositoryMock = new Mock<IItemRepository>();
            itemRepositoryMock.Setup(repo => repo.Get("key1")).ReturnsAsync(itemEntity);

            var itemService = new ItemService(itemRepositoryMock.Object);

            // Act
            var result = await itemService.Get("key1");

            // Assert
            result.Should().NotBeNull();
            result.Key.Should().Be("key1");
        }

        [Fact]
        public async Task Create_NewItem_ItemCreated()
        {
            // Arrange
            var itemCreate = _fixture.Create<ItemCreate>();
            _itemRepositoryMock.Setup(repo => repo.Get(itemCreate.Key)).ReturnsAsync((ItemEntity)null);

            // Act
            var result = await _itemServiceMock.Create(itemCreate);

            // Assert
            result.Should().Be("Item created");
            _itemRepositoryMock.Verify(repo => repo.Create(It.IsAny<ItemEntity>()), Times.Once);
            _itemRepositoryMock.Verify(repo => repo.Update(It.IsAny<ItemEntity>()), Times.Never);
        }

        [Fact]
        public async Task Create_ExistingItem_ItemUpdated()
        {
            // Arrange
            var itemCreate = _fixture.Create<ItemCreate>();
            var existingItem = _fixture.Create<ItemEntity>();
            _itemRepositoryMock.Setup(repo => repo.Get(itemCreate.Key)).ReturnsAsync(existingItem);

            // Act
            var result = await _itemServiceMock.Create(itemCreate);

            // Assert
            result.Should().Be("Item updated");
            _itemRepositoryMock.Verify(repo => repo.Update(It.IsAny<ItemEntity>()), Times.Once);
            _itemRepositoryMock.Verify(repo => repo.Create(It.IsAny<ItemEntity>()), Times.Never);
        }

        [Fact]
        public async Task Update_ExistingItem_ItemUpdated()
        {
            // Arrange
            var itemCreate = _fixture.Create<ItemCreate>();
            var existingItem = _fixture.Create<ItemEntity>();
            _itemRepositoryMock.Setup(repo => repo.Get(itemCreate.Key)).ReturnsAsync(existingItem);

            // Act
            var result = await _itemServiceMock.Update(itemCreate);

            // Assert
            result.Should().Be("Item updated");
            _itemRepositoryMock.Verify(repo => repo.Update(It.IsAny<ItemEntity>()), Times.Once);
            _itemRepositoryMock.Verify(repo => repo.Create(It.IsAny<ItemEntity>()), Times.Never);
        }

        [Fact]
        public async Task Update_NewItem_ItemCreated()
        {
            // Arrange
            var itemCreate = _fixture.Create<ItemCreate>();
            _itemRepositoryMock.Setup(repo => repo.Get(itemCreate.Key)).ReturnsAsync((ItemEntity)null);

            // Act
            var result = await _itemServiceMock.Update(itemCreate);

            // Assert
            result.Should().Be("Item created");
            _itemRepositoryMock.Verify(repo => repo.Create(It.IsAny<ItemEntity>()), Times.Once);
            _itemRepositoryMock.Verify(repo => repo.Update(It.IsAny<ItemEntity>()), Times.Never);
        }

        [Fact]
        public async Task Delete_ExistingItem_ItemDeleted()
        {
            // Arrange
            string key = "someKey";
            ItemEntity existingItem = new ItemEntity { Value = "[\"string\"]" };
            _itemRepositoryMock.Setup(repo => repo.Get(key)).ReturnsAsync(existingItem);
            _itemRepositoryMock.Setup(repo => repo.Delete(key));

            // Act
            await _itemServiceMock.Delete(key);

            // Assert
            _itemRepositoryMock.Verify(repo => repo.Delete(key), Times.Once);
        }

        [Fact]
        public async Task Delete_NonExistingItem_NoException()
        {
            //Arrange
           string key = "nonExistingKey";
            _itemRepositoryMock.Setup(repo => repo.Get(key)).ReturnsAsync((ItemEntity)null);
            _itemRepositoryMock.Setup(repo => repo.Delete(key));

            // Act
            Func<Task> act = async () => await _itemServiceMock.Delete(key);

            // Assert
            await _itemServiceMock.Invoking(x => x.Delete(It.Is<string>(x => x == key)))
                .Should().ThrowAsync<NotFoundException>();

            _itemRepositoryMock.Verify(repo => repo.Delete(key), Times.Never);
        }
    }
}
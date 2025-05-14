using System.Text.Json;
using StackExchange.Redis;

namespace Dometrain.Monolith.Api.Courses;

public class CachedCourseRepository : ICourseRepository
{
    private readonly ICourseRepository _courseRepository;
    private readonly IConnectionMultiplexer _connectionMultiplexer;

    public CachedCourseRepository(ICourseRepository courseRepository, IConnectionMultiplexer connectionMultiplexer)
    {
        _courseRepository = courseRepository;
        _connectionMultiplexer = connectionMultiplexer;
    }

    public async Task<Course?> CreateAsync(Course course)
    {
        var created = await _courseRepository.CreateAsync(course);
        if (created is null)
        {
            return null;
        }

        var db = _connectionMultiplexer.GetDatabase();
        var serializedCourse = JsonSerializer.Serialize(course);
        await db.StringSetAsync($"course_id_{course.Id}", serializedCourse);
        return created;
    }

    public async Task<Course?> GetByIdAsync(Guid id)
    {
        return await _courseRepository.GetByIdAsync(id);
    }

    public async Task<Course?> GetBySlugAsync(string slug)
    {
        return await _courseRepository.GetBySlugAsync(slug);
    }

    public async Task<IEnumerable<Course>> GetAllAsync(string nameFilter, int pageNumber, int pageSize)
    {
        return await _courseRepository.GetAllAsync(nameFilter, pageNumber, pageSize);
    }

    public async Task<Course?> UpdateAsync(Course course)
    {
        return await _courseRepository.UpdateAsync(course);
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        return await _courseRepository.DeleteAsync(id);
    }
}

using StackExchange.Redis;
using System.Text.Json;

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
        var batch = new KeyValuePair<RedisKey, RedisValue>[]
        {
            new($"course_id_{course.Id}", serializedCourse),
            new($"course_slug_{course.Slug}", course.Id.ToString())
        };
        await db.StringSetAsync(batch);
        return created;
    }

    public async Task<Course?> GetByIdAsync(Guid id)
    {
        //return await _courseRepository.GetByIdAsync(id);

        var db = _connectionMultiplexer.GetDatabase();
        var cachedCourse = await db.StringGetAsync($"course_id_{id}");
        if (!cachedCourse.IsNull)
        {
            return JsonSerializer.Deserialize<Course>(cachedCourse.ToString());
        }

        var course = await _courseRepository.GetByIdAsync(id);
        if (course is null)
        {
            return course;
        }
        var serializedCourse = JsonSerializer.Serialize(course); var batch = new KeyValuePair<RedisKey, RedisValue>[]
        {
            new($"course_id_{course.Id}", serializedCourse),
            new($"course_slug_{course.Slug}", course.Id.ToString())
        };
        await db.StringSetAsync(batch);

        return course;
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
        //return await _courseRepository.UpdateAsync(course);

        var updated = await _courseRepository.UpdateAsync(course);
        if (updated is null)
        {
            return updated;
        }

        var db = _connectionMultiplexer.GetDatabase();
        var serializedCourse = JsonSerializer.Serialize(course);
        var batch = new KeyValuePair<RedisKey, RedisValue>[]
        {
            new($"course_id_{course.Id}", serializedCourse),
            new($"course_slug_{course.Slug}", course.Id.ToString())
        };
        await db.StringSetAsync(batch);
        return updated;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        //return await _courseRepository.DeleteAsync(id);

        var deleted = await _courseRepository.DeleteAsync(id);

        if (!deleted)
        {
            return deleted;
        }

        var db = _connectionMultiplexer.GetDatabase();
        var cachedCourseString = await db.StringGetAsync($"course_id_{id}");
        if (cachedCourseString.IsNull)
        {
            return deleted;
        }
        var course = JsonSerializer.Deserialize<Course>(cachedCourseString!)!;
        var deletedCache = await db.KeyDeleteAsync([$"course_id_{id}", $"course_slug_{course.Slug}"]);
        return deletedCache > 0;
    }
}

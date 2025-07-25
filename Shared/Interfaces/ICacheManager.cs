using System;

namespace course_service.Shared.Interfaces;

public interface ICacheManager
{
    public Task<bool?> RemoveByPattern(string pattern);
}

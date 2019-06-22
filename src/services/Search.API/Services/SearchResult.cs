using System.Collections.Generic;

namespace Search.API.Services
{
    public class SearchResult<T>
    {
        public int Total { get; set; }
        public int Page { get; set; }
        public IEnumerable<T> Results { get; set; }
        public int ElapsedMilliseconds { get; set; }
    }

}

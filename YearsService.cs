using Microsoft.EntityFrameworkCore;

namespace WebApplication1
{
    public class YearsService
    {
        
        private readonly Database _db;
        public YearsService(Database db)
        {
            _db = db ?? throw new ArgumentNullException(nameof(db));
        }
        public List<string> YearsList()
        {
            if (!_db.TestConnection())
            {
                Console.WriteLine("Не удалось подключиться к базе данных");
                return new List<string>();
            }


            var years = _db.Movies.Select(m => m.Years).ToList();



            return years;
        }
    }
}

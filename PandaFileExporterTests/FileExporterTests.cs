using Microsoft.EntityFrameworkCore;
using PandaFileExporter;
using PandaTech.IEnumerableFilters;
using PandaTech.IEnumerableFilters.Dto;

namespace PandaFileExporterTests
{
    public class FileExporterTests
    {
        private readonly AppDbContext _context;
        public FileExporterTests()
        {
            var builder = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: "testDB")
                .Options;
            _context = new AppDbContext(builder);

            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
        }

        private void InitializeDb()
        {
            _context.Models.Add(new Model
            {
                Id = 1,
                Name = "Foo",
            });
            _context.SaveChanges();
        }

        [Fact]
        public void Export_Xlsx()
        {
            InitializeDb();

            var request = new GetDataRequest
            {
                Filters = new()
                {
                    new FilterDto(){ PropertyName = "Name", Values = new List<object>(){ "Foo" }, ComparisonType = ComparisonType.Equal },
                },
                Aggregates = new(),
                Order = new()
            };

            // todo: check why the List<object> doesn't convert to json.string via filters
            var test = request.ToString();
            var test2 = GetDataRequest.FromString(test);

            var response = FileExporter.ExportToXlsx(_context.Models.ApplyFilters(test2.Filters));

            Assert.NotNull(response.Content);
            Assert.NotEmpty(response.Content.Headers);
        }

        [Fact]
        public void Export_Xls()
        {
            InitializeDb();

            var request = new GetDataRequest
            {
                Filters = new()
                {
                    new FilterDto(){ PropertyName = "Name", Values = new List<object>(){ (object)"Foo" }, ComparisonType = ComparisonType.Equal },
                },
                Aggregates = new(),
            };

            // todo: check why the List<object> doesn't convert to json.string via filters
            var test = request.ToString();
            var test2 = GetDataRequest.FromString(test);

            var response = FileExporter.ExportToXls(_context.Models.ApplyFilters(test2.Filters));

            Assert.NotNull(response.Content);
            Assert.NotEmpty(response.Content.Headers);
        }

        [Fact]
        public void Export_Csv()
        {
            InitializeDb();

            var request = new GetDataRequest
            {
                Filters = new()
                {
                    new FilterDto(){ PropertyName = "Name", Values = new List<object>(){ (object)"Foo" }, ComparisonType = ComparisonType.Equal },
                },
                Aggregates = new(),
                Order = new()
            };

            // todo: check why the List<object> doesn't convert to json.string via filters
            var test = request.ToString();
            var test2 = GetDataRequest.FromString(test);

            var response = FileExporter.ExportToCsv(_context.Models.ApplyFilters(test2.Filters));

            Assert.NotNull(response.Content);
            Assert.NotEmpty(response.Content.Headers);
        }

        [Fact]
        public void Export_PDF()
        {
            InitializeDb();

            var request = new GetDataRequest
            {
                Filters = new(),
                Aggregates = new(),
                Order = new()
            };

            var response = FileExporter.ExportToPdf(_context.Models.ApplyFilters(request.Filters));

            Assert.NotNull(response.Content);
            Assert.NotEmpty(response.Content.Headers);
        }

        [Fact]
        public void ToExcelArray()
        {
            InitializeDb();

            var request = new GetDataRequest
            {
                Filters = new(),
                Aggregates = new(),
                Order = new()
            };

            var response = FileExporter.ToExcelArray(_context.Models.ApplyFilters(request.Filters));

            Assert.NotNull(response);
        }

        [Fact]
        public void ToPdfArray()
        {
            InitializeDb();

            var request = new GetDataRequest
            {
                Filters = new(),
                Aggregates = new(),
                Order = new()
            };

            var response = FileExporter.ToPdfArray(_context.Models.ApplyFilters(request.Filters));

            Assert.NotNull(response);
        }


        [Fact]
        public void test()
        {
            var now = DateTime.UtcNow;

            var id = "1_25_1324567891234";

            // Convert to int
            string ticks = now.Ticks.ToString(); // Get

            // Convert to datetime
            DateTime dateTime = new DateTime(Convert.ToInt64(ticks)); // Set
            
            Assert.True(now == dateTime);
        }
    }
}
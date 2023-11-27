using System.Reflection;
using ExcelExporter;
using Microsoft.EntityFrameworkCore;
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
                .UseInMemoryDatabase(databaseName: "testDB1")
                .Options;
            _context = new AppDbContext(builder);

            _context.Database.EnsureDeleted();
            _context.Database.EnsureCreated();
        }

        private void InitializeDb()
        {
            _context.Models.Add(new DbModel
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
        public void ToExcelArray_List()
        {
            InitializeDb();

            var request = new GetDataRequest
            {
                Filters = new(),
                Aggregates = new(),
                Order = new()
            };

            var response = FileExporter.ToExcelArray(_context.Models.ApplyFilters(request.Filters).ToList());

            Assert.NotNull(response);
        }

        [Fact]
        public void ToCsvArray_List()
        {
            InitializeDb();

            var request = new GetDataRequest
            {
                Filters = new(),
                Aggregates = new(),
                Order = new()
            };

            var response = FileExporter.ToCsvArray(_context.Models.ApplyFilters(request.Filters).ToList());

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
        public void ToPdfArray_List()
        {
            InitializeDb();

            var request = new GetDataRequest
            {
                Filters = new(),
                Aggregates = new(),
                Order = new()
            };
            var data = _context.Models.ApplyFilters(request.Filters).ToList();
            var response = FileExporter.ToPdfArray(_context.Models.ApplyFilters(request.Filters).ToList());

            Assert.NotNull(response);
        }

        [Fact]
        public void Property_DisplayName_Attribute()
        {
            var model = new DbModel();

            foreach (var item in model.GetType().GetProperties())
            {
                Assert.Equal($"Model {item.Name}", item.GetDisplayName());
            }
        }

        [Fact]
        public void Property_Custom_DisplayName_Attribute()
        {
            var model = new DtoModel()
            {
                Id = 1,
                DbModels = new List<DbModel>
                {
                    new DbModel{Id = 1, Name = "Name 1"},
                    new DbModel{Id = 2, Name = "Name 2"},
                    new DbModel{Id = 3, Name = "Name 3"},
                }
            };

            foreach (var item in model.GetType().GetProperties())
            {
                var atts = item.GetCustomAttributes(typeof(CustomDisplayNameAttribute), true);
                var neededValue = atts.Length == 0 ? item.Name : (atts[0] as CustomDisplayNameAttribute)!.DisplayName;

                var existingValue = item.GetCustomDisplayName();
                
                Assert.Equal(neededValue, existingValue); }
        }

        [Fact]
        public void Class_DisplayName_Attribute()
        {
            var model = new DbModel();

            var name = model.GetDisplayName();

            //Assert.Equal("DB Model", name);
            Assert.Equal("Model Custom Name", name);
        }

        [Fact]
        public void testtsts()
        {
            
            var ticks = "638224542870360200";
            long.TryParse(ticks, out long result3);
            //var newDate = new DateTime(result3, DateTimeKind.Utc);
            var newDate = new DateTime(result3);

            //var existingDate = new DateTime(date, DateTimeKind.Utc);

            var date = "2023-06-15 19:31:27.03602+00";
            DateTime.TryParse(date, out DateTime existingDate);
            //existingDate = DateTime.SpecifyKind(existingDate, DateTimeKind.Local).ToUniversalTime();
            existingDate = existingDate.ToUniversalTime();

            Assert.Equal(existingDate, newDate);
            Assert.Equal(existingDate.Ticks, newDate.Ticks);
        }
    }
}
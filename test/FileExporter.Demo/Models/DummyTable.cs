﻿using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;

namespace FileExporter.Demo.Models;

[DisplayName("Dummy Table")]
public class DummyTable
{
   [DisplayName("Dummy ID")]
   public long Id { get; set; }

   [DisplayName("Related ID")]
   public long? RelatedId { get; set; }

   [DisplayName("Name - long name which must me 1-31")]
   public string Name { get; set; } = null!;

   public string Status { get; set; } = "Created";
   public int Price { get; set; } = 50000;
   public int Count { get; set; } = 100;
   public int? Min { get; set; } = 0;
   public long? Average { get; set; } = 500;
   public int? Max { get; set; } = 0;
   public string Description { get; set; } = "Test, test";

   [DisplayName("Creation Date")]
   public DateTime CreationDate { get; set; } = DateTime.UtcNow;

   [DisplayName("Expiration Date")]
   public DateTime ExpirationDate { get; set; } = DateTime.UtcNow.AddDays(10);

   public string? Comment { get; set; }
   public DateTime Version { get; set; } = DateTime.UtcNow;

   [NotMapped]
   public List<DateTime> Dates { get; set; } = new()
   {
      DateTime.Now,
      DateTime.Today,
      DateTime.UtcNow
   };

   public List<string>? NullableList { get; set; }

   [NotMapped]
   public DTO DTO { get; set; } = new()
   {
      Name = "Name"
   };

   [NotMapped]
   public List<DTO>? Dtos { get; set; } = new()
   {
      new DTO
      {
         Id = 1,
         Name = "1"
      }
   };

   [NotMapped]
   public MyEnum Enum { get; set; } = MyEnum.A;

   [NotMapped]
   public MyEnum[] EnumArray { get; set; } =
   {
      MyEnum.A,
      MyEnum.B,
      MyEnum.C,
      MyEnum.D
   };
}
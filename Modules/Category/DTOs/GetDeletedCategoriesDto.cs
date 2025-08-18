using System;
using System.ComponentModel.DataAnnotations;
using course_service.Shared.DTOs;

namespace course_service.Modules.Category.DTOs;

public class GetDeletedCategoriesDto : PaginationDto
{
    // [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [DataType(DataType.Date)]
    public DateTime? StartDate { get; set; }

    // [DisplayFormat(DataFormatString = "{0:yyyy-MM-dd}", ApplyFormatInEditMode = true)]
    [DataType(DataType.Date)]
    public DateTime? EndDate { get; set; }
}

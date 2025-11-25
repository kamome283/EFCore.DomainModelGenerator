using Microsoft.EntityFrameworkCore;
using SampleProgram.Entities;

namespace SampleProgram.Domains;

public partial class StaffsDomain
{
  // User defined domain logic
  public IAsyncEnumerable<Staff> GetStaffsOnDateAsync(DateTime date)
  {
    var start = date.Date;
    var end = date.AddDays(1);
    return StaffSchedules
      .Where(x => start <= x.Start && x.Start < end)
      .Include(x => x.Staff.Details)
      .Select(x => x.Staff)
      .ToAsyncEnumerable();
  }
}

using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace ERP.Entity.Settings;

public class InventorySettings
{
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public Guid Id { get; set; }
    public bool AutoReserveOnReceive { get; set; }
    public bool AutoFulfillOnReceive { get; set; }
    public bool AutoRunMrpOnReceive { get; set; }
    public bool AllowPartialFulfillment { get; set; }
    public bool AllowOverFulfill { get; set; }
}
namespace StealME.Server.Frontend.Web.Models
{
    using System.ComponentModel.DataAnnotations;

    public class DeviceRegistrationModel
    {
        [Required]
        [Display(Name = "Device name")]
        public string DeviceName { get; set; }

        [Required]
        [Display(Name = "Description")]
        public string Description { get; set; }

        [Display(Name = "IMEI")]
        public string IMEI { get; set; }
    }

}
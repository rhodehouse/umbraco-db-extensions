# Umbraco Database Extensions
Compatible with UmbracoCms.Core v7.12.2
## Documentation
### Create a class
class CalendarEvent
{
        [NullSetting(NullSetting = NullSettings.NotNull)]
        public DateTime Date_TS { get; set; }  
}

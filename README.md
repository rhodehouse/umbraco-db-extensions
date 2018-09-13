# Umbraco Database Extensions
Compatible with UmbracoCms.Core v7.12.2
## Documentation 
Sync a model class to a database table. Changes on the class will trigger the database to be updated when executed.
### Create a class
```
[PrimaryKey("Id", autoIncrement = true)]
internal class CalendarEvent
{
    private string _event;
    private EventType _eventType;

    [NullSetting(NullSetting = NullSettings.NotNull)]
    public DateTime Date_TS { get; set; }

    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Description { get; set; }

    /// <summary>
    /// Associated with EventType enum
    /// string representation saved in the database
    /// </summary>
    [Length(20)]
    [NullSetting(NullSetting = NullSettings.NotNull)]
    public string Event
    {
        get { return _event ?? ""; }
        set
        {
            if (!Enum.TryParse(value, out _eventType))
            {
                throw new NotImplementedException($"The Event Type {value} has not been implemented.");
            }
            _event = value;
        }
    }

    /// <summary>
    /// Not saved in the database. For Developer use.
    /// </summary>
    [Ignore]
    public EventType EventType
    {
        get { return _eventType; }
        set
        {
            _eventType = value;
            _event = Enum.GetName(_eventType.GetType(), _eventType);
        }
    }

    [Length(100)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string HeroImagePath { get; set; }

    [PrimaryKeyColumn(Name = "pk_CalendarEvent_Id", AutoIncrement = true)]
    public int ID { get; set; }

    [Length(100)]
    [NullSetting(NullSetting = NullSettings.Null)]
    public string ThumbImagePath { get; set; }
}
 ```  
 ### Create a Factory
 ```
 var factory = new DatabaseFactory(ApplicationCotext.Current)
 ```
 ### Sync the table
 ```
 DatabaseManager.SyncTable<CalendarEvent>(factory);
 ```

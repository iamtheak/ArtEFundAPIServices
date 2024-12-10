namespace ArtEFundAPIServices.DTO;

public class BaseResponseModel<T>
{
    public T Data { get; set; }
    
    public string Message { get; set; }
}
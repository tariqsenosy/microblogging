using System.Net;

namespace Microblogging.Shared;

public class PaginatedOutPut<T>
{
    public PaginatedOutPut(IEnumerable<T>? data, int totalItems, int? page, int? pageSize = 10,
        ResponseCode responseCode = ResponseCode.Success, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        Data = data;
        TotalItems = totalItems;
        IsSuccess = responseCode == ResponseCode.Success;
        pageSize ??= 10;
        var totalPages = (int)Math.Ceiling((decimal)totalItems / (decimal)(pageSize ?? 10));
        var currentPage = page ?? 1;
        var startPage = currentPage - 5;
        var endPage = currentPage + 4;
        if (startPage <= 0)
        {
            endPage -= (startPage - 1);
            startPage = 1;
        }

        if (endPage > totalPages)
        {
            endPage = totalPages;
            if (endPage > 10)
            {
                startPage = endPage - 9;
            }
        }

        TotalItems = totalItems;
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalPages = totalPages;
        StartPage = startPage;
        EndPage = endPage;
        StatusCode = statusCode;
    }


    public IEnumerable<T>? Data { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public bool IsSuccess { get; set; }
    public int TotalItems { get; set; }
    public int CurrentPage { get; private set; }
    public int? PageSize { get; private set; }
    public int TotalPages { get; private set; }
    public int StartPage { get; private set; }
    public int EndPage { get; private set; }
}

public class PaginatedOutPutWithoutList<T>
{
    public PaginatedOutPutWithoutList(T? data, int totalItems, int? page, int? pageSize = 10,
        ResponseCode responseCode = ResponseCode.Success, HttpStatusCode statusCode = HttpStatusCode.OK)
    {
        Data = data;
        TotalItems = totalItems;
        IsSuccess = responseCode == ResponseCode.Success;
        pageSize ??= 10;
        var totalPages = (int)Math.Ceiling(totalItems / (decimal)pageSize);
        var currentPage = page ?? 1;
        var startPage = currentPage - 5;
        var endPage = currentPage + 4;
        if (startPage <= 0)
        {
            endPage -= (startPage - 1);
            startPage = 1;
        }

        if (endPage > totalPages)
        {
            endPage = totalPages;
            if (endPage > 10)
            {
                startPage = endPage - 9;
            }
        }

        TotalItems = totalItems;
        CurrentPage = currentPage;
        PageSize = pageSize;
        TotalPages = totalPages;
        StartPage = startPage;
        EndPage = endPage;
        StatusCode = statusCode;
    }


    public T? Data { get; set; }
    public HttpStatusCode StatusCode { get; set; }
    public bool IsSuccess { get; set; }
    public int TotalItems { get; set; }
    public int CurrentPage { get; private set; }
    public int? PageSize { get; private set; }
    public int TotalPages { get; private set; }
    public int StartPage { get; private set; }
    public int EndPage { get; private set; }
}
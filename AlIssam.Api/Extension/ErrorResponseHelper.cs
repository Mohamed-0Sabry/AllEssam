using AlIssam.API.Dtos.Response;

namespace AlIssam.API.Extension
{
    public static class ErrorResponseHelper
    {

        public static ErrorResponse CreateErrorResponse(this ErrorResponse error, string arErrro, string enError)
        {
            var err  = new ErrorResponse
            { 
                Errors = new Errors
                { 
                    Ar = new List<string> {arErrro },
                    En = new List<string> { enError } 
                } 
            };
            return err;
             
        }

        public static ErrorResponse AddError(this ErrorResponse error, string arErrro, string enError)
        {
            error.Errors.Ar.Add(arErrro);
            error.Errors.En.Add(enError);
            return error;
         
        }
    }
}

namespace AlIssam.API.Dtos
{
    public class ResultViewModel
    {
        public bool IsSucceeded { get; set; }
        public List<string> Messages { get; set; }


    }
    public class Result<t> : ResultViewModel
    {
        public t Data { get; set; }
        public t Ar { get; set; }
        public t En { get; set; }

        public static Result<t> Success(t data, List<string> messages)// override
        {
            return new()
            {
                Data = data,
                IsSucceeded = true,
                Messages = messages
            };
        }

        public static Result<t> Success(params string[] messages)
        {
            return new()
            {
                Messages = messages.Any() ? messages.ToList() : null,
                IsSucceeded = true
            };
        }

        public static Result<t> Success(t data)
        {
            return new()
            {
                Data = data,
                IsSucceeded = true,

            };
        }

        public static Result<t> Success(t Ar, t Eng)// override
        {
            return new()
            {
                Ar = Ar,
                En = Eng,
                IsSucceeded = true

            };
        }
        public static Result<t> Faild(t data)
        {
            return new()
            {
                Data = data,
                IsSucceeded = false,

            };
        }

        public static Result<t> Failed(Exception exception)
        {
            var messages = new List<string> { exception.Message };
            if (exception.InnerException != null)
            {
                messages.AddRange(exception.InnerException.Message.Split(Environment.NewLine));
            }

            return new Result<t>
            {
                Messages = messages,
                IsSucceeded = false
            };
        }
        public static Result<t> Failed(params string[] messages)
        {
            return new()
            {
                Messages = messages.Any() ? messages.ToList() : null,
                IsSucceeded = false
            };
        }


    }
}

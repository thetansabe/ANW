using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ANWebServices.SignalR.Models
{
    public class ResponseMessage
    {
        public ResponseMessage()
        {
            status = ResponseStatus.OK;
        }
        public ResponseMessage(ResponseStatus status)
        {
            this.status = status;
        }
        public ResponseMessage(object msg, ResponseStatus status=ResponseStatus.OK)
        {
            this.status = status;
            data = msg;
        }
        public ResponseStatus status { get; set; }
        public object data { get; set; }
    }
    public enum ResponseStatus
    {
        OK=200, FORBID=403, UNAUTHORIZED=401, BAD_REQUEST=400, NOT_FOUND=404, NO_CONTENT=204
    }
}

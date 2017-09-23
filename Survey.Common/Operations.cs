using System;
using System.Collections.Generic;
using System.Linq;
namespace Survey.Common
{
    public class Operations
    {
        // Survey management operations
        public const string CREATE = "create";
        public const string EDIT = "edit";
        public const string CLOSE = "close";
        public const string DELETE = "delete";

        // Survey response operations
        public const string RESPOND = "respond";
        public const string UPDATE_RESPONSE = "update_response";

        // Survey query operations
        public const string GET_SURVEYS = "get_surveys";
        public const string GET_SURVEY_DETAILS = "get_survey_details";
        public const string GET_SURVEY_RESPONSES = "get_survey_responses";
    }
}
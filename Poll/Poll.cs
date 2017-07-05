using AntShares.SmartContract.Framework;
using AntShares.SmartContract.Framework.Services.AntShares;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace AntShares.SmartContract
{
    public class Poll : FunctionCode
    {
        internal const int POLL_ID_LENGTH = 32;

        private const string POLL_ID_LIST_KEY = "polls";

        #region Operation constants
        // Poll management operations
        private const string CREATE_OPERATION = "create";
        private const string EDIT_OPERATION = "edit";
        private const string CLOSE_OPERATION = "close";
        private const string DELETE_OPERATION = "delete";

        // Poll response operations
        private const string RESPOND_OPERATION = "respond";
        private const string UPDATE_RESPONSE_OPERATION = "update_response";

        // Poll query operations
        private const string GET_POLLS_OPERATION = "get_polls";
        private const string GET_POLL_RESPONSES_OPERATION = "get_poll_responses";
        #endregion Operation constants

        private const int MAX_QUESTION_LENGTH = 128;
        private const int MAX_POLL_RESPONSE_OPTIONS = 30;

        public static object Main(string operation, params object[] args)
        {
            switch (operation)
            {
                // Poll management operations
                case CREATE_OPERATION:
                    return CreatePoll((byte[]) args[0], (byte[]) args[1], (string) args[2], (string[]) args[3]);
                case CLOSE_OPERATION:
                    return ClosePoll((byte[]) args[0], (byte[]) args[1], (byte[]) args[2]);
                case EDIT_OPERATION:
                    return EditPoll((byte[]) args[0], (byte[]) args[1], (byte[]) args[2]);
                case DELETE_OPERATION:
                    return DeletePoll((byte[]) args[0], (byte[]) args[1], (byte[]) args[2]);

                // Poll response operations
                case RESPOND_OPERATION:
                    return RespondToPoll((byte[]) args[0], (byte[]) args[1], (byte[]) args[2], (byte) args[3]);
                case UPDATE_RESPONSE_OPERATION:
                    return UpdatePollResponse((byte[])args[0], (byte[])args[1], (byte[])args[2], (byte)args[3]);

                // Poll querying operations
                case GET_POLLS_OPERATION:
                    return GetPolls();
                case GET_POLL_RESPONSES_OPERATION:
                    return GetPollResponses((byte[]) args[0]);

                default:
                    return false;
            }
        }

        #region Poll management operations
        /// <summary>
        /// Create a new poll
        /// </summary>
        /// <param name="poller">Poll conductor's address</param>
        /// <param name="signature">Poller's signature</param>
        /// <param name="question">Poll question</param>
        /// <param name="options">Poll response options</param>
        /// <returns>Poll identifier if successfully created, otherwise error</returns>
        private static byte[] CreatePoll(byte[] poller, byte[] signature, string question, string[] options)
        {
            if (!VerifySignature(poller, signature)) return null;

            // Validate question length
            if (question.Length > MAX_QUESTION_LENGTH) return null;

            // Validate number of response options
            if (options.Length > MAX_POLL_RESPONSE_OPTIONS) return null;


            // Poll is valid, get poll identifier and create poll

            // Hash question to create identifier
            var pollId = Sha256(Encoding.UTF8.GetBytes(question));

            // Ensure it is a unique identifier
            var uniqueIdFound = false;
            while (!uniqueIdFound)
            {
                var pollWithId = Storage.Get(Storage.CurrentContext, pollId);

                if (pollWithId != null)
                {
                    // Identifier is being used

                    // Try get a unique identifier by hashing hash
                    pollId = Sha256(pollId);
                }
                else
                {
                    uniqueIdFound = true;
                }
            }

            // Unique poll identifier generated, create poll

            // Assign identifiers to response options
            var optionsWithIds = new Dictionary<byte, string>();
            byte optionId = 0;
            foreach (var option in options)
            {
                optionsWithIds.Add(optionId, option);
                optionId++;
            }

            // Encode poll
            var poll = PollEncoder.Encode(true, poller, question, optionsWithIds);

            // Check encoding was successful
            if (poll == null) return null;

            // Store poll
            Storage.Put(Storage.CurrentContext, pollId, poll);

            // Add to list of polls
            AddToPollIdList(pollId);

            // Poll created, return poll identifier
            return pollId;
        }

        private static bool EditPoll(byte[] poller, byte[] signature, byte[] pollId)
        {
            var poll = Storage.Get(Storage.CurrentContext, pollId);

            if (poll == null) return false;

            if (!VerifySignature(poller, signature)) return false;

            // TODO Implement


            return true;
        }

        private static bool ClosePoll(byte[] poller, byte[] signature, byte[] pollId)
        {
            var poll = Storage.Get(Storage.CurrentContext, pollId);

            if (poll == null) return false;

            if (!VerifySignature(poller, signature)) return false;

            // TODO Implement


            return true;
        }

        private static bool DeletePoll(byte[] poller, byte[] signature, byte[] pollId)
        {
            var poll = Storage.Get(Storage.CurrentContext, pollId);

            if (poll == null) return false;

            if (!VerifySignature(poller, signature)) return false;

            Storage.Delete(Storage.CurrentContext, pollId);

            // Remove from list of polls
            RemoveFromPollIdList(pollId);

            return true;
        }
        #endregion Poll management operations

        #region Poll response operations
        private static bool RespondToPoll(byte[] responder, byte[] signature, byte[] pollId, byte response)
        {
            if (!VerifySignature(responder, signature)) return false;

            var encodedPoll = Storage.Get(Storage.CurrentContext, pollId);

            var updatedPoll = PollEncoder.AddResponse(encodedPoll, responder, response);

            // Successfully responded to poll
            return true;
        }
                
        private static bool UpdatePollResponse(byte[] responder, byte[] signature, byte[] pollId, byte newResponse)
        {
            if (!VerifySignature(responder, signature)) return false;

            var encodedPoll = Storage.Get(Storage.CurrentContext, pollId);

            if (encodedPoll == null) return false;

            // Decode poll
            bool isOpen;
            byte[] poller;
            string question;
            Dictionary<byte, string> options;
            Dictionary<byte[], byte> responses;
            PollEncoder.Decode(encodedPoll, out isOpen, out poller, out question, out options, out responses);

            // Check responder has responded previously
            if (!responses.ContainsKey(responder)) return false;

            // Replace with new response
            responses[responder] = newResponse;

            // Encode updated poll
            var encodedUpdatedPoll = PollEncoder.Encode(isOpen, poller, question, options, responses);

            // Store encoded poll
            Storage.Put(Storage.CurrentContext, pollId, encodedUpdatedPoll);

            // Successfully updated response to poll
            return true;
        }
        #endregion Poll response operations
        
        #region Poll querying methods
        private static byte[] GetPollResponses(byte[] pollId)
        {
            var poll = Storage.Get(Storage.CurrentContext, pollId);

            if (poll == null) return null;

            // Get responses
            bool isOpen;
            byte[] poller;
            string question;
            Dictionary<byte, string> options;
            Dictionary<byte[], byte> responses;

            var decodedSuccessfully = PollEncoder.Decode(poll, out isOpen, out poller, out question, out options, out responses);

            if (!decodedSuccessfully) return null;

            // Count number of responses for each option
            var responseCounts = new Dictionary<byte, uint>();

            foreach (var response in responses)
            {
                if (!responseCounts.ContainsKey(response.Value))
                {
                    // Add first response for option
                    responseCounts.Add(response.Value, 1);
                }
                else
                {
                    // Increment response count for option
                    responseCounts[response.Value]++;
                }
            }

            // Encode response counts
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    // Write number of response counts
                    writer.Write(responseCounts.Count);

                    foreach (var responseCount in responseCounts)
                    {
                        // Write option identifier
                        writer.Write(responseCount.Key);

                        // Write response count
                        writer.Write(responseCount.Value);
                    }

                    // Encoded successfully
                    writer.Flush();
                    return stream.ToArray();
                }
            }
        }

        private static byte[] GetPolls()
        {
            var pollIds = GetPollIdList();

            // Build encoded list of poll identifiers and their corresponding questions
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    // Write number of polls
                    writer.Write(pollIds.Count);

                    foreach (var pollId in pollIds)
                    {
                        // Get encoded poll
                        var encodedPoll = Storage.Get(Storage.CurrentContext, pollId);

                        // Get poll question
                        var pollQuestion = PollEncoder.DecodeQuestion(encodedPoll);

                        var encodedQuestion = Encoding.UTF8.GetBytes(pollQuestion);

                        
                        // Write poll identifier
                        writer.Write(pollId);

                        // Write encoded question length
                        writer.Write(encodedQuestion.Length);

                        // Write encoded question
                        writer.Write(encodedQuestion);
                    }

                    // Encoded successfully
                    writer.Flush();
                    return stream.ToArray();
                }
            }
        }
        #endregion Poll querying methods

        #region Poll identifier list methods
        private static bool AddToPollIdList(byte[] pollId)
        {
            var pollIdList = GetPollIdList();

            try
            {
                // Check if identifier is already in the list
                var pollIdAlreadyInList = pollIdList.Any(id => id.SequenceEqual(pollId));

                if (pollIdAlreadyInList) return false;

                // Add poll identifier to list
                pollIdList.Add(pollId);

                UpdatePollIdList(pollIdList);

                // Added successfully
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static bool RemoveFromPollIdList(byte[] pollId)
        {
            var pollIdList = GetPollIdList();

            try
            {
                // Remove poll identifier from list
                pollIdList.Remove(pollId);

                UpdatePollIdList(pollIdList);

                // Removed successfully
                return true;
            }
            catch
            {
                return false;
            }
        }

        private static List<byte[]> GetPollIdList()
        {
            var encodedPollIdList = Storage.Get(Storage.CurrentContext, POLL_ID_LIST_KEY);

            var pollIdList = new List<byte[]>();

            if (encodedPollIdList == null) return pollIdList;

            // Decode to list of poll identifiers
            using (var stream = new MemoryStream())
            {
                using (var reader = new BinaryReader(stream))
                {
                    // Read number of poll identifiers
                    var pollIdCount = reader.ReadUInt32();

                    for (uint i = 0; i < pollIdCount; i++)
                    {
                        // Read poll identifier
                        var pollId = reader.ReadBytes(POLL_ID_LENGTH);

                        pollIdList.Add(pollId);
                    }

                    // Decoded successfully
                    return pollIdList;
                }
            }
        }

        private static bool UpdatePollIdList(List<byte[]> newPollIdList)
        {
            if (newPollIdList == null) return false;

            // Encode list of poll identifiers
            using (var stream = new MemoryStream())
            {
                using (var writer = new BinaryWriter(stream))
                {
                    // Write number of poll identifiers
                    writer.Write((uint) newPollIdList.Count);

                    foreach (var pollId in newPollIdList)
                    {
                        // Write poll identifier
                        writer.Write(pollId);                       
                    }

                    // Encoded successfully
                    writer.Flush();
                    var encodedPollIdList = stream.ToArray();

                    // Store new encoded poll identifier list
                    Storage.Put(Storage.CurrentContext, POLL_ID_LIST_KEY, encodedPollIdList);

                    return true;
                }
            }
        }
        #endregion Poll identifier list methods
    }
}
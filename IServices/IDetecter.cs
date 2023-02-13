using BraimChallenge.Models;

namespace BraimChallenge.IServices
{
    public interface IDetecter
    {
        public string[] HeaderData(string header);
        public int DetectUserAuth(string Authorize);
        public int DetectAccountId(int? accountId);
        public int DetectAccount(int? accountId, string Authorize, List<Account> accountList);

    }
}

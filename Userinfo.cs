namespace BiliOnlineRank
{
    public class Userinfo
    {
       
            /// <summary>
            /// 账号
            /// </summary>
            public string account { get; set; }
            /// <summary>
            /// 密码
            /// </summary>
            public string password { get; set; }
            public string captchtxt { get; set; }
        public Userinfo(string account, string password,string captchtxt)
            {
                this.account = account;
                this.password = password;
            this.captchtxt = captchtxt;
            }
        public Userinfo(string account, string password)
        {
            this.account = account;
            this.password = password;
            this.captchtxt = captchtxt;
        }
        public Userinfo(string captchtxt)
        {
      
            this.captchtxt = captchtxt;
        }



    }
}
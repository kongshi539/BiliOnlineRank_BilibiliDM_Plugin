using System;
using System.Text;
using Bili;
using Bili.Exceptions;
using Bili.Models;
using System.IO;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using BiliOnlineRank;
using System.Windows.Forms;
using System.Net.Sockets;
using System.Net;
/// <summary>
/// 本插件获取高能榜数据部分使用了 xuan25/BiliOnlineRank  https://github.com/xuan25/BiliOnlineRank 的代码
/// 因为某些需求，在此基础上增加了通过弹幕姬进行管理账号和开关的功能
/// ！！！修改过后的验证码功能未经过测试！！！
/// </summary>
namespace DMPluginTest
{
 
    public class Class1 : BilibiliDM_PluginFramework.DMPlugin
    {
    
        public Class1()
        {
        
            this.Connected += Class1_Connected;
            this.Disconnected += Class1_Disconnected;
            this.ReceivedDanmaku += Class1_ReceivedDanmaku;
            this.ReceivedRoomCount += Class1_ReceivedRoomCount;
            this.PluginAuth = "业玖";
            this.PluginName = "弹幕狐提词板高能榜显示插件";
            this.PluginCont = "example@example.com";
            this.PluginVer = "v0.0.1";
        }


        private void Class1_ReceivedRoomCount(object sender, BilibiliDM_PluginFramework.ReceivedRoomCountArgs e)
        {
        }

        private void Class1_ReceivedDanmaku(object sender, BilibiliDM_PluginFramework.ReceivedDanmakuArgs e)
        {
           
        }

        private void Class1_Disconnected(object sender, BilibiliDM_PluginFramework.DisconnectEvtArgs e)
        {
            if (BliveOnline.t1 != null)
                BliveOnline.t1.Abort();
        }

        private void Class1_Connected(object sender, BilibiliDM_PluginFramework.ConnectedEvtArgs e)
        {
            
        }

        public override void Admin()
        {
            base.Admin();
            BliveOnline.FormLogin = new Form1();
            string[] lines = File.ReadAllLines("onlineconfig.txt");
            this.Log(lines[0]);
            this.Log(lines[1]);
            this.Log(lines[2]);
    
            BliveOnline.FormLogin.Show();

        }

        public override void Stop()
        {
            base.Stop();
            //請勿使用任何阻塞方法
            BliveOnline.IsRun = false;
            Console.WriteLine("Plugin Stoped!");
            this.Log("Plugin Stoped!");
            this.AddDM("Plugin Stoped!", true);
        }

        public override void Start()
        {
            base.Start();
            BliveOnline.IsRun = true;
            //請勿使用任何阻塞方法
            BliveOnline.Start();
           
            //启动登录 会阻塞
            Console.WriteLine("Plugin Started!");
            this.Log("Plugin Started!");
            this.AddDM("Plugin Started!", true);
        }
    }
}


public static class BliveOnline
{
    /// <summary>
    /// Read user input from console as password
    /// </summary>
    /// <returns>password</returns>
    ///  
    public static Form1 FormLogin;
    public static Form2 FormCaptcha;
    public static Thread captchThread;
    public static LoginInfo loginInfo;
    public static string username;
     public static string password;
    public static string prefix;
    public static Thread t1;
    
    public static Boolean IsRun = true;
    public static void captchInput(object sender, Userinfo uinfo)
    {
        try {
            FormCaptcha.Close();
            string captchtxt = uinfo.captchtxt;
            loginInfo = BiliLogin.Login(username, password, captchtxt);
            Console.WriteLine(loginInfo);
            Console.WriteLine();
            string defaultPrefix = "http://localhost:6689/";
            void startsvc()
            {
                // Refresh token
                // TODO: store and re-use the token 
                Console.WriteLine("-------- 更新令牌 --------");
                LoginToken newLoginToken = BiliLogin.RefreshToken(loginInfo.Token);
                Console.WriteLine(newLoginToken);
                Console.WriteLine();

                // Room Info
                Console.WriteLine("-------- 房间信息 --------");
                RoomInfo roomInfo = BiliLive.GetInfo(loginInfo.Token.AccessToken, loginInfo.Token.Mid.ToString());
                Console.WriteLine($"房间号: {roomInfo.RoomId}");
                Console.WriteLine($"用户ID: {roomInfo.Uid}");
                Console.WriteLine($"用户名: {roomInfo.Uname}");
                Console.WriteLine($"标题: {roomInfo.Title}");
                Console.WriteLine($"分区名称: {roomInfo.ParentName} - {roomInfo.AreaV2Name}");
                Console.WriteLine($"粉丝牌名称: {roomInfo.MedalName}");
                Console.WriteLine();

                // Service
                Console.WriteLine("-------- 本地服务 --------");
                ApiProvider apiProvider = new ApiProvider(defaultPrefix);
                apiProvider.Start();
                Console.WriteLine($"服务运行在: {defaultPrefix}");
                Console.WriteLine($"  /data: 获取数据");

                // Ranking list
                Console.WriteLine("-------- 排行榜 --------");
                Console.WriteLine();
                while (true)
                {
                    if (IsRun)
                    {
                        Console.WriteLine("排名\t贡献值\t用户名");
                        Console.WriteLine("-------- 高能榜 --------");
                        AnchorOnlineGoldRank onlineGoldRank = BiliLive.GetAnchorOnlineGoldRank(loginInfo.Token.AccessToken, "1", "50", roomInfo.RoomId.ToString(), loginInfo.Token.Mid.ToString());
                        apiProvider.GoldRank = onlineGoldRank;
                        foreach (AnchorOnlineGoldRankItem item in onlineGoldRank.Items)
                        {
                            Console.WriteLine($"{item.UserRank}\t{item.Score}\t{item.Name}");
                        }

                        Console.WriteLine("-------- 在线用户 --------");
                        OnlineRank onlineRank = BiliLive.GetOnlineRank(loginInfo.Token.AccessToken, "1", "50", roomInfo.RoomId.ToString(), loginInfo.Token.Mid.ToString());
                        apiProvider.Rank = onlineRank;
                        foreach (OnlineRankItem item in onlineRank.Items)
                        {
                            Console.WriteLine($"-\t0\t{item.Name}");
                        }
                        Console.WriteLine($"在线人数: {onlineRank.OnlineNum}");
                        Console.WriteLine();

                        // Sleep for 20s
                        //将间隔时间改为了10s 2021/8/28 YEJIU
                        for (int i = 10; i > 0; i--)
                        {
                            Console.Write($"\r{i} ");
                            Thread.Sleep(1000);
                        }
                        Console.WriteLine($"\r  ");

                    }
                    else break;
                }
                apiProvider.Stop();

            }
            t1 = new Thread(new ThreadStart(startsvc));
            t1.Start();
        }
         catch (LoginFailedException ex)
        {
            if (File.Exists("onlineconfig.txt"))
                File.Delete("onlineconfig.txt");
            Form3 formError= new Form3();
            formError.Show();
            Console.WriteLine(ex);
            
        }
        catch (LoginStatusException ex)
        {
            if (File.Exists("onlineconfig.txt"))
                File.Delete("onlineconfig.txt");
            Form3 formError = new Form3();
            formError.Show();
            Console.WriteLine(ex);
            Console.WriteLine(ex);
            
        }
    }
    public static void loginconfig(object sender, Userinfo uinfo)
    {
         
        Console.WriteLine("*注: 用户名为手机号");
        Console.WriteLine("*注: 登录信息将会自动保存到 config.txt");
        Console.Write("用户名: ");
        string username = uinfo.account;
        Console.Write("密码: ");
        string password = uinfo.password;
        if (password == null)
        {
            Console.Error.WriteLine("登录中断");
            return;
        }
        Console.WriteLine($" <隐藏>");
        //删除了自定义本地服务器功能
        //Console.Write($"本地服务前缀 (默认为 {defaultPrefix}): ");

        string prefix = "";
        File.WriteAllLines("onlineconfig.txt", new string[] { username, password, prefix });
        FormLogin.Close();

        Start();
    }
    private static string ReadPasswordFromConsole()
    {
        StringBuilder passwordBuilder = new StringBuilder();
        {
            while (true)
            {
                ConsoleKeyInfo keyInfo = Console.ReadKey(true);
                ConsoleKey key = keyInfo.Key;
                if (key == ConsoleKey.Enter)
                {
                    break;
                }
                else if (key == ConsoleKey.Escape)
                {
                    return null;
                }
                else if (key == ConsoleKey.Backspace)
                {
                    if (passwordBuilder.Length > 0)
                    {
                        passwordBuilder.Remove(passwordBuilder.Length - 1, 1);
                    }
                }
                else
                {
                    passwordBuilder.Append(keyInfo.KeyChar);
                }
            }
        }
        return passwordBuilder.ToString();
    }

    /// <summary>
    /// Show a image in a new window
    /// </summary>
    /// <param name="image">image</param>
    /// <returns>Window thread</returns>
    public static Thread ShowImage(BitmapImage image)
    {
        Thread windowThread = new Thread(() =>
        {
            Window window = new Window()
            {
                Title = "验证码",
                Width = image.Width * 2,
                Height = image.Height * 2 + 36,
                Content = new Image() { Source = image }
            };
            window.ShowDialog();
        });
        windowThread.SetApartmentState(ApartmentState.STA);
        windowThread.Start();

        return windowThread;
    }


    /// <summary>
    /// Entry point
    /// </summary>
    /// <param name="args"></param>
    public static void Start()
    {   //出于应用需要，固定使用http://localhost:6689/作为地址  021/8/28 YEJIU
        string defaultPrefix = "http://localhost:6689/";
        // Init
        Console.WriteLine("-------- 信息 --------");
        string username, password, prefix;
        if (File.Exists("onlineconfig.txt"))
        {
            // Has login info
            string[] lines = File.ReadAllLines("onlineconfig.txt");
            username = lines[0];
            password = lines[1];
            prefix = lines[2];
            Console.WriteLine($"用户名: <隐藏>");
            Console.WriteLine($"密码: <隐藏>");
            Console.WriteLine($"本地服务前缀: {prefix}");
        }
        else
        {    //添加了交互界面 2021/8/28 YEJIU
             //没有confi（初次登录或删除了）
             // Require login info
            FormLogin = new Form1();
            FormLogin.Show();
            return;

        }



        // Login
        loginInfo = null;
        try
        {
            // Normal login
            Console.WriteLine("-------- 普通登录 --------");
            loginInfo = BiliLogin.Login(username, password);
            Console.WriteLine(loginInfo);
            Console.WriteLine();
        }
        catch (LoginFailedException ex)
        {
            Console.WriteLine(ex);
        }
        catch (LoginStatusException ex)
        {
            Console.WriteLine(ex);
        }

        if (loginInfo == null)
        {
            try
            {///
             ///验证码窗体没做，先留着
             ///做了，但是我登录不报验证码，测试不了，先留着
             ///
                // Login with captcha
                Console.WriteLine("-------- 验证码登录 --------");
                BitmapImage captchaImage = BiliLogin.GetCaptcha();

                captchThread = ShowImage(captchaImage);
                Console.Write("请输入验证码: ");
                FormCaptcha = new Form2();
                FormCaptcha.Show();
         
            }
            catch (LoginFailedException ex)
            {
                Console.WriteLine(ex);
            }
            catch (LoginStatusException ex)
            {
                Console.WriteLine(ex);
            }
        }

        if (loginInfo == null)
        {
            Console.Error.WriteLine("登录失败");
            return;
        }
        // 子线程运行
        void startsvc()
        {
            // Refresh token
            // TODO: store and re-use the token 
            Console.WriteLine("-------- 更新令牌 --------");
            LoginToken newLoginToken = BiliLogin.RefreshToken(loginInfo.Token);
            Console.WriteLine(newLoginToken);
            Console.WriteLine();

            // Room Info
            Console.WriteLine("-------- 房间信息 --------");
            RoomInfo roomInfo = BiliLive.GetInfo(loginInfo.Token.AccessToken, loginInfo.Token.Mid.ToString());
            Console.WriteLine($"房间号: {roomInfo.RoomId}");
            Console.WriteLine($"用户ID: {roomInfo.Uid}");
            Console.WriteLine($"用户名: {roomInfo.Uname}");
            Console.WriteLine($"标题: {roomInfo.Title}");
            Console.WriteLine($"分区名称: {roomInfo.ParentName} - {roomInfo.AreaV2Name}");
            Console.WriteLine($"粉丝牌名称: {roomInfo.MedalName}");
            Console.WriteLine();

            // Service
            Console.WriteLine("-------- 本地服务 --------");
            ApiProvider apiProvider = new ApiProvider(defaultPrefix);
            apiProvider.Start();
            Console.WriteLine($"服务运行在: {defaultPrefix}");
            Console.WriteLine($"  /data: 获取数据");

            // Ranking list
            Console.WriteLine("-------- 排行榜 --------");
            Console.WriteLine();
            while (true)
            {
                if (IsRun)
                {
                    Console.WriteLine("排名\t贡献值\t用户名");
                    Console.WriteLine("-------- 高能榜 --------");
                    AnchorOnlineGoldRank onlineGoldRank = BiliLive.GetAnchorOnlineGoldRank(loginInfo.Token.AccessToken, "1", "50", roomInfo.RoomId.ToString(), loginInfo.Token.Mid.ToString());
                    apiProvider.GoldRank = onlineGoldRank;
                    foreach (AnchorOnlineGoldRankItem item in onlineGoldRank.Items)
                    {
                        Console.WriteLine($"{item.UserRank}\t{item.Score}\t{item.Name}");
                    }

                    Console.WriteLine("-------- 在线用户 --------");
                    OnlineRank onlineRank = BiliLive.GetOnlineRank(loginInfo.Token.AccessToken, "1", "50", roomInfo.RoomId.ToString(), loginInfo.Token.Mid.ToString());
                    apiProvider.Rank = onlineRank;
                    foreach (OnlineRankItem item in onlineRank.Items)
                    {
                        Console.WriteLine($"-\t0\t{item.Name}");
                    }
                    Console.WriteLine($"在线人数: {onlineRank.OnlineNum}");
                    Console.WriteLine();

                    // Sleep for 20s
                    //将间隔时间改为了10s 2021/8/28 YEJIU
                    for (int i = 10; i > 0; i--)
                    {
                        Console.Write($"\r{i} ");
                        Thread.Sleep(1000);
                    }
                    Console.WriteLine($"\r  ");

                }
                else break;
            }
            apiProvider.Stop();

        }
        
        
            if (IsRun&& loginInfo!=null) { 
             t1 = new Thread(new ThreadStart(startsvc));

            t1.Start();
            }

        
    }
}


 
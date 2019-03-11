//import BaseCmd;

//public class MainActivity {

//    // Used to load the 'native-lib' library on application startup.
//    static {
//        System.loadLibrary("native-lib");
//    }

//    public static MainActivity Instance;

//    TextView mOutputTextView;
//    @Override
//    protected void onCreate(Bundle savedInstanceState) {
//        super.onCreate(savedInstanceState);
//        setContentView(R.layout.activity_main);

//        Instance = this;
//        // Example of a call to a native method
//        TextView tv = (TextView) findViewById(R.id.sample_text);
//        tv.setText(stringFromJNI());

//        mOutputTextView = (TextView) findViewById(R.id.TV_Output);

//        if(ContextCompat.checkSelfPermission(this, Manifest.permission.INTERNET) != PackageManager.PERMISSION_GRANTED){
//            //没有 CALL_PHONE 权限
//            ActivityCompat.requestPermissions(this,new string[]{Manifest.permission.INTERNET},1);

//        }
//        Button btnConnect = (Button)findViewById(R.id.Btn_Connect);
//        btnConnect.setOnClickListener(new View.OnClickListener() {
//            @Override
//            public void onClick(View v) {


//                TcpClient.getInstance().creatConnect("10.0.2.2",88);
//                //TcpClient.getInstance().creatConnect("127.0.0.1",88);
//                //TcpClient.getInstance().creatConnect("192.168.1.183",88);
//            }
//        });


//        Button btnSignin = (Button)findViewById(R.id.Btn_ReqSignin);
//        btnSignin.setOnClickListener(new View.OnClickListener() {
//            @Override
//            public void onClick(View v) {

//                Cmd.sUser user = Cmd.sUser.newBuilder().setName("michael").setId(99).setEmail("michael@qq.com").build();
//                Cmd.reqSignin req = Cmd.reqSignin.newBuilder().addUser(user).build();
//                try {

//                    TcpClient.getInstance().GetMessageRoute().Send(req);
//                } catch (IOException e) {
//                    e.printStackTrace();
//                }


//            }
//        });


//        Button btnMove = (Button)findViewById(R.id.Btn_ReqMove);
//        btnMove.setOnClickListener(new View.OnClickListener() {


//            private  int forward = 0;
//            @Override
//            public void onClick(View v) {

//                Cmd.reqMove req = Cmd.reqMove.newBuilder().setForward(forward++).setAngle(2).build();
//                try {

//                    TcpClient.getInstance().GetMessageRoute().Send(req);
//                } catch (IOException e) {
//                    e.printStackTrace();
//                }


//            }
//        });




//    }


//    BroadcastReceiver mReceiver = new BroadcastReceiver() {
//        @Override
//        public void onReceive(Context context, Intent intent) {
//            // Do what you need in here

//            mOutputTextView.setText(intent.getExtras().getString(Intent.EXTRA_TEXT));
//        }
//    };
//    @Override
//    protected void onDestroy() {
//        super.onDestroy();
//        try {
//            unregisterReceiver(mReceiver);
//            TcpClient.getInstance().disconnect();
//        } catch (IOException e) {
//            e.printStackTrace();
//        }
//    }
//    @Override
//    protected void onResume() {
//        super.onResume();
//        registerReceiver(mReceiver, new IntentFilter(BaseProcessor.MessageFilter));
//    }

//    /**
//     * A native method that is implemented by the 'native-lib' native library,
//     * which is packaged with this application.
//     */
//    public native string stringFromJNI();
//}

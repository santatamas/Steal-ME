package stealme.remote.main;

import android.os.Bundle;
import android.app.Activity;
import android.view.Menu;
import android.view.MotionEvent;
import android.view.View;
import android.widget.Button;
import android.widget.TextView;

public class MainActivity extends Activity {

	private Button _btnActivate;
	private Button _btnDeactivate;
	private Button _btnGetStatus;
	private Button _btnSignal;
	private TextView _txtPhoneNumber;
	
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);
        
        _btnActivate = (Button) findViewById(R.id.btnActivate);
        _btnDeactivate = (Button) findViewById(R.id.btnDeactivate);
        _btnGetStatus = (Button) findViewById(R.id.btnGetStatus);
        _btnSignal = (Button) findViewById(R.id.btnSignal);
        _txtPhoneNumber = (TextView) findViewById(R.id.txtPhoneNumber);
       
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        getMenuInflater().inflate(R.menu.activity_main, menu);
        return true;
    }
}

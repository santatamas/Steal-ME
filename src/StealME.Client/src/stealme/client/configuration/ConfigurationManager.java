package stealme.client.configuration;

import stealme.client.main.R;
import android.os.Bundle;
import android.preference.PreferenceActivity;

public class ConfigurationManager extends PreferenceActivity {
    @Override
    public void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);  
 
        addPreferencesFromResource(R.xml.configurationmanager);
    }
}

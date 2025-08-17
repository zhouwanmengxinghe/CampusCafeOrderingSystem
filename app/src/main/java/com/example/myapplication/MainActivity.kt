package com.example.myapplication

import android.content.Intent
import android.os.Bundle
import androidx.activity.ComponentActivity
import androidx.activity.compose.setContent
import androidx.core.view.WindowCompat
import androidx.compose.foundation.layout.fillMaxSize
import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.Surface
import androidx.compose.ui.Modifier
import com.example.myapplication.ui.theme.MyApplicationTheme

class MainActivity : ComponentActivity() {
    override fun onCreate(savedInstanceState: Bundle?) {
        super.onCreate(savedInstanceState)
        WindowCompat.setDecorFitsSystemWindows(window, false)
        
        // Handle dialing Intent from external sources
        val phoneNumber = when (intent?.action) {
            Intent.ACTION_DIAL -> {
                intent.data?.schemeSpecificPart ?: ""
            }
            else -> ""
        }
        
        setContent {
            MyApplicationTheme {
                Surface(
                    modifier = Modifier.fillMaxSize(),
                    color = MaterialTheme.colorScheme.background
                ) {
                    DialerScreen()
                }
            }
        }
    }
    
    override fun onNewIntent(intent: Intent?) {
        intent?.let { super.onNewIntent(it) }
        setIntent(intent)
        // Handle new Intent, such as dialing requests from other apps
    }
}

private fun ComponentActivity.onNewIntent(intent: Intent?) {}

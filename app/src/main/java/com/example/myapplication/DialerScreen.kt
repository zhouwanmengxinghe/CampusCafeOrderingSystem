package com.example.myapplication

import android.Manifest
import android.content.Context
import android.content.Intent
import android.content.pm.PackageManager
import android.net.Uri
import androidx.activity.compose.rememberLauncherForActivityResult
import androidx.activity.result.contract.ActivityResultContracts
import androidx.compose.foundation.background
import androidx.compose.foundation.layout.*
import androidx.compose.foundation.shape.CircleShape
import androidx.compose.foundation.shape.RoundedCornerShape
import androidx.compose.material3.*
import androidx.compose.runtime.*
import androidx.compose.ui.Alignment
import androidx.compose.ui.Modifier
import androidx.compose.ui.graphics.Color
import androidx.compose.ui.platform.LocalContext
import androidx.compose.ui.text.font.FontWeight
import androidx.compose.ui.text.style.TextAlign
import androidx.compose.ui.unit.dp
import androidx.compose.ui.unit.sp
import androidx.core.content.ContextCompat

@Composable
fun DialerScreen() {
    var phoneNumber = remember { mutableStateOf("") }
    val context = LocalContext.current
    
    // Permission request launcher
    val permissionLauncher = rememberLauncherForActivityResult(
        contract = ActivityResultContracts.RequestPermission()
    ) { isGranted ->
        if (isGranted) {
            if (phoneNumber.value.isNotEmpty()) {
                val intent = Intent(Intent.ACTION_CALL)
                intent.data = Uri.parse("tel:" + phoneNumber.value)
                context.startActivity(intent)
            }
        }
    }

    val columnModifier = Modifier
        .fillMaxSize()
        .background(Color.White)
        .padding(16.dp)
    
    Column(
        modifier = columnModifier,
        horizontalAlignment = Alignment.CenterHorizontally
    ) {
        // Top status bar area
        val topRowModifier = Modifier
            .fillMaxWidth()
            .padding(vertical = 8.dp)
        
        Row(
            modifier = topRowModifier,
            horizontalArrangement = Arrangement.SpaceBetween,
            verticalAlignment = Alignment.CenterVertically
        ) {
            Text(text = "9:41", fontSize = 16.sp, fontWeight = FontWeight.Medium)
            Row {
                Text(text = "📶", fontSize = 14.sp)
                Spacer(modifier = Modifier.width(4.dp))
                Text(text = "📶", fontSize = 14.sp)
                Spacer(modifier = Modifier.width(4.dp))
                Text(text = "🔋", fontSize = 14.sp)
            }
        }

        Spacer(modifier = Modifier.height(40.dp))

        // Phone number display area
        val displayText = if (phoneNumber.value.isEmpty()) "123456" else phoneNumber.value
        Text(
            text = displayText,
            fontSize = 32.sp,
            fontWeight = FontWeight.Light,
            color = Color.Black,
            textAlign = TextAlign.Center,
            modifier = Modifier.padding(vertical = 20.dp)
        )

        Spacer(modifier = Modifier.height(40.dp))

        // Digital button grid
        val gridModifier = Modifier.padding(horizontal = 20.dp)
        val rowModifier = Modifier.padding(bottom = 20.dp)
        
        Column(
            horizontalAlignment = Alignment.CenterHorizontally,
            modifier = gridModifier
        ) {
            // First row: 1, 2, 3
            Row(
                horizontalArrangement = Arrangement.spacedBy(20.dp),
                modifier = rowModifier
            ) {
                DialerButtonWithLetters(
                    number = "1", 
                    letters = "", 
                    onClick = { phoneNumber.value = phoneNumber.value + "1" }
                )
                DialerButtonWithLetters(
                    number = "2", 
                    letters = "ABC", 
                    onClick = { phoneNumber.value = phoneNumber.value + "2" }
                )
                DialerButtonWithLetters(
                    number = "3", 
                    letters = "DEF", 
                    onClick = { phoneNumber.value = phoneNumber.value + "3" }
                )
            }
            
            // Second row: 4, 5, 6
            Row(
                horizontalArrangement = Arrangement.spacedBy(20.dp),
                modifier = rowModifier
            ) {
                DialerButtonWithLetters(
                    number = "4", 
                    letters = "GHI", 
                    onClick = { phoneNumber.value = phoneNumber.value + "4" }
                )
                DialerButtonWithLetters(
                    number = "5", 
                    letters = "JKL", 
                    onClick = { phoneNumber.value = phoneNumber.value + "5" }
                )
                DialerButtonWithLetters(
                    number = "6", 
                    letters = "MNO", 
                    onClick = { phoneNumber.value = phoneNumber.value + "6" }
                )
            }
            
            // Third row: 7, 8, 9
            Row(
                horizontalArrangement = Arrangement.spacedBy(20.dp),
                modifier = rowModifier
            ) {
                DialerButtonWithLetters(
                    number = "7", 
                    letters = "PQRS", 
                    onClick = { phoneNumber.value = phoneNumber.value + "7" }
                )
                DialerButtonWithLetters(
                    number = "8", 
                    letters = "TUV", 
                    onClick = { phoneNumber.value = phoneNumber.value + "8" }
                )
                DialerButtonWithLetters(
                    number = "9", 
                    letters = "WXYZ", 
                    onClick = { phoneNumber.value = phoneNumber.value + "9" }
                )
            }
            
            // Fourth row: *, 0, #
            Row(
                horizontalArrangement = Arrangement.spacedBy(20.dp),
                modifier = rowModifier
            ) {
                DialerButtonWithLetters(
                    number = "*", 
                    letters = "", 
                    onClick = { phoneNumber.value = phoneNumber.value + "*" }
                )
                DialerButtonWithLetters(
                    number = "0", 
                    letters = "+", 
                    onClick = { phoneNumber.value = phoneNumber.value + "0" }
                )
                DialerButtonWithLetters(
                    number = "#", 
                    letters = "", 
                    onClick = { phoneNumber.value = phoneNumber.value + "#" }
                )
            }
        }

        Spacer(modifier = Modifier.height(40.dp))

        // Call button
        val callButtonModifier = Modifier
            .width(120.dp)
            .height(50.dp)
        
        val greenColor = Color(0xFF4CAF50)
        val buttonColors = ButtonDefaults.buttonColors(containerColor = greenColor)
        val buttonShape = RoundedCornerShape(25.dp)
        
        Button(
            onClick = { 
                if (phoneNumber.value.isNotEmpty()) {
                    makePhoneCall(phoneNumber.value, context, permissionLauncher)
                }
            },
            colors = buttonColors,
            shape = buttonShape,
            modifier = callButtonModifier
        ) {
            Text(
                text = "Call", 
                color = Color.White, 
                fontSize = 18.sp,
                fontWeight = FontWeight.Medium
            )
        }
    }
}

@Composable
fun DialerButtonWithLetters(
    number: String,
    letters: String,
    onClick: () -> Unit
) {
    val buttonModifier = Modifier.size(80.dp)
    val blueColor = Color(0xFF5C6BC0)
    val buttonColors = ButtonDefaults.buttonColors(
        containerColor = blueColor,
        contentColor = Color.White
    )
    
    Button(
        onClick = onClick,
        modifier = buttonModifier,
        colors = buttonColors,
        shape = CircleShape
    ) {
        Column(
            horizontalAlignment = Alignment.CenterHorizontally,
            verticalArrangement = Arrangement.Center
        ) {
            Text(
                text = number,
                fontSize = 24.sp,
                fontWeight = FontWeight.Medium,
                color = Color.White
            )
            if (letters.isNotEmpty()) {
                Text(
                    text = letters,
                    fontSize = 10.sp,
                    fontWeight = FontWeight.Normal,
                    color = Color.White
                )
            }
        }
    }
}

private fun makePhoneCall(
    phoneNumber: String,
    context: Context,
    permissionLauncher: androidx.activity.compose.ManagedActivityResultLauncher<String, Boolean>
) {
    val hasPermission = ContextCompat.checkSelfPermission(context, Manifest.permission.CALL_PHONE)
    
    if (hasPermission == PackageManager.PERMISSION_GRANTED) {
        val intent = Intent(Intent.ACTION_CALL)
        intent.data = Uri.parse("tel:" + phoneNumber)
        context.startActivity(intent)
    } else {
        permissionLauncher.launch(Manifest.permission.CALL_PHONE)
    }
}
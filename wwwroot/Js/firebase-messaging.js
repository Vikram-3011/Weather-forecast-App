import { initializeApp } from "firebase/app";
import { getMessaging, getToken, onMessage } from "firebase/messaging";

const firebaseConfig = {
    apiKey: "AIzaSyAjdZer9TDWUPDhHTJJxyIymJ1kT0ZQ3CQ",
    authDomain: "weather-alert-2dfbf.firebaseapp.com",
    projectId: "weather-alert-2dfbf",
    storageBucket: "weather-alert-2dfbf.appspot.com",
    messagingSenderId: "794442006903",
    appId: "1:794442006903:web:44b6b17c697bd55d112172"
};

const app = initializeApp(firebaseConfig);
const messaging = getMessaging(app);

// Define and export your function
export function requestNotificationPermission(userEmail) {
    Notification.requestPermission().then((permission) => {
        if (permission === "granted") {
            getToken(messaging, { vapidKey: "BCKCzWA0fRNeg3nC3h9-E7KmA6Pqia5YmxwdZolHI-IaI9RlUtqBs69LENV9UAsiuP0bdAFUbkYXtdbTSV2JEVw" })
                .then((currentToken) => {
                    if (currentToken) {
                        console.log("FCM Token:", currentToken);
                        // Send token & email to backend
                        fetch('/api/notifications/register', {
                            method: 'POST',
                            headers: { 'Content-Type': 'application/json' },
                            body: JSON.stringify({ userEmail, token: currentToken })
                        })
                            .then(res => res.json())
                            .then(data => {
                                console.log("Token stored:", data);
                            })
                            .catch(err => console.error("Error storing token:", err));
                    } else {
                        console.log("No registration token available.");
                    }
                })
                .catch((err) => {
                    console.error("Error retrieving token:", err);
                });
        } else {
            console.log("Notification permission denied.");
        }
    });
}

// Expose the function globally so Blazor can find it
window.requestNotificationPermission = requestNotificationPermission;

// Handle foreground messages
onMessage(messaging, (payload) => {
    console.log("Message received:", payload);
    new Notification(payload.notification.title, {
        body: payload.notification.body,
        icon: payload.notification.icon || "/default-icon.png"
    });
});

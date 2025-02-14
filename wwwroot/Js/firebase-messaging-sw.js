importScripts("https://www.gstatic.com/firebasejs/9.6.10/firebase-app.js");
importScripts("https://www.gstatic.com/firebasejs/9.6.10/firebase-messaging.js");

firebase.initializeApp({
    apiKey: "AIzaSyAjdZer9TDWUPDhHTJJxyIymJ1kT0ZQ3CQ",
    authDomain: "weather-alert-2dfbf.firebaseapp.com",
    projectId: "weather-alert-2dfbf",
    storageBucket: "weather-alert-2dfbf.appspot.com",
    messagingSenderId: "794442006903",
    appId: "1:794442006903:web:44b6b17c697bd55d112172"
});

const messaging = firebase.messaging();

messaging.onBackgroundMessage((payload) => {
    console.log("Received background message:", payload);

    self.registration.showNotification(payload.notification.title, {
        body: payload.notification.body,
        icon: payload.notification.icon || "/default-icon.png"
    });
});

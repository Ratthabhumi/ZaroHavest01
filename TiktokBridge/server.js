// server.js (ฉบับขึ้นไลฟ์จริง: ตัดคำสั่ง Test ทิ้ง)
const { WebcastPushConnection } = require('tiktok-live-connector');
const WebSocket = require('ws');

// 🔴🔴🔴 แก้ชื่อช่อง TikTok ของเฮียตรงนี้ 🔴🔴🔴
let tiktokUsername = "mewmewnakub";

// ========================================================
// 1. สร้าง WebSocket Server
const wss = new WebSocket.Server({ port: 8080 });

wss.on('connection', function connection(ws) {
    console.log('✅ Unity Game Connected!');
});

function sendToUnity(data) {
    const jsonString = JSON.stringify(data);
    wss.clients.forEach(function each(client) {
        if (client.readyState === WebSocket.OPEN) {
            client.send(jsonString);
        }
    });
}

// 2. เชื่อมต่อ TikTok Live
let tiktokLiveConnection = new WebcastPushConnection(tiktokUsername);

function connectToTikTok() {
    tiktokLiveConnection.connect().then(state => {
        console.info(`🚀 Connected to TikTok RoomId: ${state.roomId}`);
    }).catch(err => {
        console.error('❌ Failed to connect (Retrying in 5s)...', err);
        setTimeout(connectToTikTok, 5000);
    });
}
connectToTikTok();

// ========================================================
// 🎯 โซนตั้งค่า Event (เหลือแค่ระบบหลัก)
// ========================================================

// 1. ส่งของขวัญ (Gift) -> พระเอกของงาน
tiktokLiveConnection.on('gift', (data) => {
    // กรองการส่งซ้ำ
    if (data.giftType === 1 && !data.repeatEnd) {
        return;
    }

    let displayName = data.nickname || data.uniqueId;
    console.log(`🎁 [GIFT] ${displayName} sent ${data.giftName}`);

    sendToUnity({
        // ZaroHarvest
        type: 'gift',
        name: displayName,
        msg: data.giftName,
        count: 1,

        // RNG GOD
        eventName: 'gift',
        username: displayName,
        giftName: data.giftName,
        giftId: data.giftId,
        avatarUrl: data.profilePictureUrl
    });
});

// 2. กดใจ (Like)
tiktokLiveConnection.on('like', (data) => {
    let displayName = data.nickname || data.uniqueId;
    console.log(`❤️ [LIKE] ${displayName} x${data.likeCount}`);

    sendToUnity({
        type: 'like',
        name: displayName,
        count: data.likeCount,
        eventName: 'like',
        username: displayName,
        avatarUrl: data.profilePictureUrl
    });
});

// 3. กดติดตาม (Follow)
tiktokLiveConnection.on('follow', (data) => {
    let displayName = data.nickname || data.uniqueId;
    console.log(`➕ [FOLLOW] ${displayName}`);

    sendToUnity({
        type: 'follow',
        name: displayName,
        eventName: 'follow',
        username: displayName,
        count: 1
    });
});

// 4. แชท (Chat) - เหลือไว้แค่โชว์ในจอดำ (แต่ไม่ส่งคำสั่งหมุนไปเกมแล้ว)
tiktokLiveConnection.on('chat', (data) => {
    let displayName = data.nickname || data.uniqueId;
    console.log(`💬 ${displayName}: ${data.comment}`);
    // ❌ ลบโค้ดเช็คคำว่า "roll" ทิ้งไปแล้ว ปลอดภัย 100%
});

console.log(`✨ Server Ready! (Production Mode - No Test Commands)`);
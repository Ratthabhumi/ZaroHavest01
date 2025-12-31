// server.js (ฉบับสมบูรณ์: รองรับ Chat Command + Like System)
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
// 🎯 โซนตั้งค่า Event
// ========================================================

// 1. ส่งของขวัญ (Gift)
tiktokLiveConnection.on('gift', (data) => {
    if (data.giftType === 1 && !data.repeatEnd) {
        return;
    }

    let displayName = data.nickname || data.uniqueId;
    console.log(`🎁 [GIFT] ${displayName} sent ${data.giftName}`);

    sendToUnity({
        // สำหรับ ZaroHarvest (เผื่อใช้)
        type: 'gift',
        name: displayName,
        msg: data.giftName,
        count: 1,

        // สำหรับ RNG GOD
        eventName: 'gift',
        username: displayName,
        giftName: data.giftName,
        giftId: data.giftId,
        avatarUrl: data.profilePictureUrl
    });
});

// 2. กดใจ (Like) - ✅ แก้ไข: เพิ่ม likeCount ให้ตรงกับ Unity
tiktokLiveConnection.on('like', (data) => {
    let displayName = data.nickname || data.uniqueId;
    // console.log(`❤️ [LIKE] ${displayName} x${data.likeCount}`); // ปิด Log หน่อยก็ได้จะได้ไม่รก

    sendToUnity({
        // ZaroHarvest
        type: 'like',
        name: displayName,
        count: data.likeCount,

        // RNG GOD
        eventName: 'like',
        username: displayName,
        avatarUrl: data.profilePictureUrl,
        likeCount: data.likeCount // 🔥 สำคัญมาก! ต้องมีตัวนี้ Unity ถึงจะนับถูก
    });
});

// 3. แชท (Chat) - ✅ แก้ไข: เปิดให้ส่งแชทไป Unity (เพื่อเช็คคำสั่ง reset/roll)
tiktokLiveConnection.on('chat', (data) => {
    let displayName = data.nickname || data.uniqueId;
    let msg = data.comment;

    console.log(`💬 ${displayName}: ${msg}`);

    sendToUnity({
        eventName: 'chat',
        username: displayName,
        avatarUrl: data.profilePictureUrl,
        comment: msg // 🔥 ส่งข้อความไปให้ Unity เช็คว่าเป็นคำสั่งไหม
    });
});

// 4. กดติดตาม (Follow)
tiktokLiveConnection.on('follow', (data) => {
    let displayName = data.nickname || data.uniqueId;
    console.log(`➕ [FOLLOW] ${displayName}`);

    sendToUnity({
        eventName: 'follow',
        username: displayName,
        count: 1
    });
});

console.log(`✨ Server Ready! (Live Mode: Chat Commands Enabled)`);
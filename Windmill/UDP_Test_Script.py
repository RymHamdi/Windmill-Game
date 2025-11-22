import socket

UDP_IP = "127.0.0.1"
UDP_PORT = 5000

print(f"UDP Test Client targeting {UDP_IP}:{UDP_PORT}")

def send_command(command):
    sock = socket.socket(socket.AF_INET, socket.SOCK_DGRAM)
    sock.sendto(bytes(command, "utf-8"), (UDP_IP, UDP_PORT))
    print(f"Sent: '{command}'")

while True:
    print("\nOptions:")
    print("1. Send 'StartGame'")
    print("2. Send 'EndGame'")
    print("3. Change Port (Current: " + str(UDP_PORT) + ")")
    print("q. Quit")
    
    choice = input("Enter choice: ")
    
    if choice == '1':
        send_command("StartGame")
    elif choice == '2':
        send_command("EndGame")
    elif choice == '3':
        new_port = input("Enter new port: ")
        try:
            UDP_PORT = int(new_port)
            print(f"Port updated to {UDP_PORT}")
        except ValueError:
            print("Invalid port number")
    elif choice == 'q':
        break


$Server = "127.0.0.1"
$Port = 25565

$Client = [System.Net.Sockets.TcpClient]::new()

try {
    Write-Host "[INFO] Connecting to Orvex Server..."

    $Client.Connect($Server, $Port)

    $Stream = $Client.GetStream()

    Write-Host "[SUCCESS] TCP connected!"

    # Handshake payload:
    # Protocol Version = 1 (Int32, Little Endian)

    $Payload = [System.BitConverter]::GetBytes([int]1)

    # Packet body:
    # Packet Type = 1 (UInt16)
    # Payload = Protocol Version

    $Body = [byte[]](
        [System.BitConverter]::GetBytes([UInt16]1) +
        $Payload
    )

    # Packet length prefix (Int32)

    $Header = [System.BitConverter]::GetBytes(
        [int]$Body.Length
    )

    $Packet = [byte[]]($Header + $Body)

    Write-Host "[INFO] Sending Handshake..."

    $Stream.Write($Packet, 0, $Packet.Length)
    $Stream.Flush()

    # Read response length
    $LengthBuffer = New-Object byte[] 4

    $Read = $Stream.Read($LengthBuffer, 0, 4)

    if ($Read -ne 4) {
        throw "Invalid response header."
    }

    $Length = [System.BitConverter]::ToInt32(
        $LengthBuffer, 0
    )

    if ($Length -lt 2 -or $Length -gt 1048576) {
        throw "Invalid response length: $Length"
    }

    # Read complete response
    $Response = New-Object byte[] $Length
    $Offset = 0

    while ($Offset -lt $Length) {
        $Count = $Stream.Read(
            $Response,
            $Offset,
            $Length - $Offset
        )

        if ($Count -eq 0) {
            throw "Connection closed by server."
        }

        $Offset += $Count
    }

    $Type = [BitConverter]::ToUInt16($Response, 0)

    if ($Type -ne 2) {
        throw "Unexpected packet type: $Type"
    }

    $Memory = [System.IO.MemoryStream]::new(
        $Response, 2, $Response.Length - 2
    )

    $Reader = [System.IO.BinaryReader]::new($Memory)

    $Accepted = $Reader.ReadBoolean()
    $Version = $Reader.ReadInt32()
    $GameVersion = $Reader.ReadString()

    Write-Host ""
    Write-Host "----- ORVEX HANDSHAKE -----"

    Write-Host "Accepted:    $Accepted"
    Write-Host "Protocol:    $Version"
    Write-Host "Game:        $GameVersion"

    if ($Accepted) {
        Write-Host ""
        Write-Host "[SUCCESS] Handshake completed!"
    }
    else {
        Write-Host ""
        Write-Host "[WARNING] Protocol version mismatch."
    }

    $Reader.Dispose()
    $Memory.Dispose()
}
catch {
    Write-Host "[ERROR] $($_.Exception.Message)" -ForegroundColor Red
}
finally {
    $Client.Close()
    Write-Host "[INFO] Connection closed."
}
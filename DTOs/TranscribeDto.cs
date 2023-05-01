namespace speech2text;

public record TranscribeDto
(
    IFormFile file,
    string model,
    bool singleLine,
    bool timestamps,
    bool statistics
);
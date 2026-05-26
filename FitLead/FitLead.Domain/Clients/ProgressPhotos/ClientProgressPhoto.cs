using FitLead.Common.Domain;
using FitLead.Common.Results;
using DomainError = FitLead.Common.Errors.Error;

namespace FitLead.Domain.Clients.ProgressPhotos
{
    public sealed class ClientProgressPhoto : AggregateRoot<Guid>
    {
        public const int MaxNoteLength = 1000;

        public Guid ClientId { get; private set; }
        public Guid MediaAssetId { get; private set; }
        public DateOnly TakenAt { get; private set; }
        public ProgressPhotoLabel Label { get; private set; }
        public string? Note { get; private set; }
        public DateTime CreatedAtUtc { get; private set; }

        private ClientProgressPhoto()
        {
        }

        private ClientProgressPhoto(
            Guid id,
            Guid clientId,
            Guid mediaAssetId,
            DateOnly takenAt,
            ProgressPhotoLabel label,
            string? note,
            DateTime createdAtUtc)
        {
            Id = id;
            ClientId = clientId;
            MediaAssetId = mediaAssetId;
            TakenAt = takenAt;
            Label = label;
            Note = note;
            CreatedAtUtc = createdAtUtc;
        }

        public static Result<ClientProgressPhoto> Create(
            Guid clientId,
            Guid mediaAssetId,
            DateOnly takenAt,
            ProgressPhotoLabel label,
            string? note,
            DateTime createdAtUtc)
        {
            if (clientId == Guid.Empty)
            {
                return Result<ClientProgressPhoto>.Failure(
                    DomainError.Validation(
                        "client_progress_photo.create.client_id_required",
                        "ClientId is required"));
            }

            if (mediaAssetId == Guid.Empty)
            {
                return Result<ClientProgressPhoto>.Failure(
                    DomainError.Validation(
                        "client_progress_photo.create.media_asset_id_required",
                        "MediaAssetId is required"));
            }

            if (takenAt == default)
            {
                return Result<ClientProgressPhoto>.Failure(
                    DomainError.Validation(
                        "client_progress_photo.create.taken_at_required",
                        "TakenAt is required"));
            }

            if (!Enum.IsDefined(label))
            {
                return Result<ClientProgressPhoto>.Failure(
                    DomainError.Validation(
                        "client_progress_photo.create.label_invalid",
                        "Label is invalid"));
            }

            if (createdAtUtc == default)
            {
                return Result<ClientProgressPhoto>.Failure(
                    DomainError.Validation(
                        "client_progress_photo.create.created_at_required",
                        "CreatedAtUtc is required"));
            }

            var noteResult = NormalizeNote(note, out var normalizedNote);
            if (noteResult.IsFailure)
            {
                return Result<ClientProgressPhoto>.Failure(noteResult.Error);
            }

            return Result<ClientProgressPhoto>.Success(
                new ClientProgressPhoto(
                    Guid.NewGuid(),
                    clientId,
                    mediaAssetId,
                    takenAt,
                    label,
                    normalizedNote,
                    createdAtUtc));
        }

        private static Result NormalizeNote(string? note, out string? normalizedNote)
        {
            if (string.IsNullOrWhiteSpace(note))
            {
                normalizedNote = null;
                return Result.Success();
            }

            var trimmedNote = note.Trim();
            if (trimmedNote.Length > MaxNoteLength)
            {
                normalizedNote = null;
                return Result.Failure(
                    DomainError.Validation(
                        "client_progress_photo.create.note_too_long",
                        $"Note cannot exceed {MaxNoteLength} characters"));
            }

            normalizedNote = trimmedNote;
            return Result.Success();
        }
    }
}

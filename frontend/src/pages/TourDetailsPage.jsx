import { useEffect, useState } from 'react'
import { useParams } from 'react-router-dom'
import BookingForm from '../components/BookingForm'

const API = '/api'

export default function TourDetailsPage() {
  const { id } = useParams()
  const [tour, setTour] = useState(null)

  useEffect(() => {
    fetch(`${API}/tours/${id}`).then((r) => r.json()).then(setTour)
  }, [id])

  if (!tour) {
    return <p>Загрузка...</p>
  }

  return (
    <section>
      <article className="card">
        <h2>{tour.title}</h2>
        <p>{tour.country}</p>
        <p>{tour.durationDays} дней</p>
        <p>{tour.description}</p>
        <strong>{tour.price} €</strong>
      </article>
      <BookingForm tourId={tour.id} />
    </section>
  )
}
